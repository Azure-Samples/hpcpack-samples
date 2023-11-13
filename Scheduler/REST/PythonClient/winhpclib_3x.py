import http.client
import ssl
import base64
from xml.dom import minidom

class Error(Exception):
    """Exception raised for errors encountered by winhpclib"""
    
    def __init__(self, status, reason, details):
        self.status = status
        self.reason = reason
        self.details = details

    def __str__(self):
        result = str(self.status)+" "+self.reason

        if(self.details):
            result += ", Details: "+str(self.details)

        return result


class Server:
    """Handles communication with Windows HPC Server via REST API"""   

    def __init__(self, headnode, username, password, port=443):
        
        self._xml_namespace = "http://schemas.microsoft.com/HPCS2008R2/common"
        
        # Build base URL string (first without the cluster name)
        self._base_url = "/WindowsHPC/"
        
        # Initialize HTTPS connection to the web service
        # WARNING: This does not do any verification of the server's certificate.
        # See http://docs.python.org/library/http.client.html
        sslcontext = ssl.SSLContext()
        sslcontext.verify_mode = ssl.CERT_NONE
        self._connection = http.client.HTTPSConnection(headnode, port, context=sslcontext)
        self._connection.connect()
        
        # Prepare basic authentication header
        auth_header_value = 'Basic {}'.format(base64.b64encode('{}:{}'.format(username, password).encode('ascii')).decode('ascii'))

        # Make authorization and api-version the default headers
        # see API-Version here: https://docs.microsoft.com/en-us/previous-versions/windows/desktop/hh560258(v%3Dvs.85)
        api_version_HPC2012         = '2012-11-01.4.0';     # for HPC Pack 2012, HPC Pack 2012 R2, HPC Pack 2016
        api_version_HPC2008R2_SP4   = '2012-03-31.3.4';     # for HPC Pack 2008 R2 with SP4
        api_version_HPC2008R2_SP3   = '2011-11-01';         # HPC Pack 2008 R2 with SP3
        
        self._default_headers = {'Authorization' : auth_header_value,
                                 'api-version'   : api_version_HPC2012}
    
    def check_successful_response(self, response):
        """Checks if provided HTTP response was successful"""
        
        # For status other than OK raise an exception
        if(response.status != 200):
            
            # Get response content
            content = response.read()

            # Extract error details if available
            details = ''            

            if(content):
                try:
                    xml_details = minidom.parseString(content)
                    xml_message = xml_details.getElementsByTagName('Message')
                    details = xml_message[0].firstChild.data
                except:
                    # In case we couldn't extract the message
                    # just return the whole response content
                    details = content
            
            raise Error(response.status, response.reason, details)
        
        
    def get_properties(self, resource_url):
        """Gets properties of the resource specified by the URL"""
        
        # Get URL and parse contents as XML
        self._connection.request('GET', self._base_url + resource_url, None, self._default_headers)
        response = self._connection.getresponse()
        self.check_successful_response(response)
        
        xmldoc = minidom.parse(response)
        
        # Get all 'Property' elements and prepare dictionary for results
        xml_properties = xmldoc.getElementsByTagName('Property')
        properties = dict()
        
        # Parse all 'Property' elements
        for xml_property in xml_properties:
            
            # Extract 'Name'
            xml_name = xml_property.getElementsByTagName('Name')
            name = xml_name[0].firstChild.data
    
            # Extract 'Value
            value = None
            xml_value = xml_property.getElementsByTagName('Value')
            if xml_value[0].firstChild != None:
                value = xml_value[0].firstChild.data
            
            # Store in the dictionary
            properties[name] = value
            
        return properties
    
    
    def create_properties_xml(self, properties):
        """"Converts provided properties dictionary to Windows HPC REST API XML format"""
            
        # Prepare xml document with root element
        xmldoc = minidom.Document()       
        root = xmldoc.createElementNS(self._xml_namespace, "ArrayOfProperty")
        xmldoc.appendChild(root)
            
        # Add properties to the document
        for key in properties:
                
            # Create xml nodes
            xml_property = xmldoc.createElementNS(self._xml_namespace, "Property")
            xml_name = xmldoc.createElementNS(self._xml_namespace, "Name")
            xml_value = xmldoc.createElementNS(self._xml_namespace, "Value")
                
            # Create inner text elements
            xml_name_text = xmldoc.createTextNode(str(key))
                
            xml_value_text = None
            if properties[key] is not None:
                xml_value_text = xmldoc.createTextNode(str(properties[key]))
                
            # Connect all elements for this property
            xml_name.appendChild(xml_name_text)
                
            if xml_value_text is not None:
                xml_value.appendChild(xml_value_text)
                
            xml_property.appendChild(xml_name)
            xml_property.appendChild(xml_value)
                
            # Add new property to document's root
            root.appendChild(xml_property)
            
        # Explicitly set namespace attribute on a root
        # element, so it will be in the output (see Python 
        # bugs 1371937 and 1621421)
        root.setAttribute("xmlns", self._xml_namespace)                
            
        return xmldoc.toxml()


    def send_properties(self, method, resource_url, properties):
        """Sends provided properties XML to the server"""
        
        # Convert properties to XML
        xml_properties = self.create_properties_xml(properties)
        
        # Send...
        return self.send_xml(method, resource_url, xml_properties) 
    
    def send_xml(self, method, resource_url, xml):
        """Sends provided XML to the server"""
        
        # Prepare headers
        headers = {"Content-type": "application/xml"}
        headers.update(self._default_headers)
        
        # Send prepared content
        self._connection.request(method, self._base_url + resource_url, xml, headers)
        response = self._connection.getresponse()
        self.check_successful_response(response)
        
        return response.read()


class HpcObject:
    """Implements common operations for Windows HPC objects"""
    
    def __init__(self, server):
        self._server = server
        self.properties = dict()
    
    def refresh_properties(self, *property_names):
        """"Refreshes all or selected object properties"""
        
        # Get url of this object
        url = self._get_url()
        
        # If getting only selected properties add 'Properties'
        # query string to the object's resource url
        if len(property_names) > 0:
            url += '?Properties=' + ','.join(property_names)
        
        # Get values from server and merge with local properties
        current_properties = self._server.get_properties(url)
        self.properties.update(current_properties)
        
    def save_properties(self, *property_names):
        """"Updates object's property values on the server"""
        
        # By default update with all local values
        selected_properties = self.properties
        
        # But if specific names provided, use subset of
        # locally stored properties
        if len(property_names) > 0:
            selected_properties = dict([ (k, self.properties[k]) for k in property_names ])
        
        # Send to the server with PUT
        self._server.send_properties("PUT", self._get_url(), selected_properties)
        
    def create(self):
        """"Creates instance of an object on the server"""
        
        # To create, POST with object creation url and currently set properties
        response = self._server.send_properties("POST", self._get_create_url(), self.properties)
                 
        # Server responds with <int>{ID}</int>
        xml_id = minidom.parseString(response)
        
        # Extract ID of the response and allow specific
        # object type to handle it value
        if(xml_id.firstChild is not None):
            if(xml_id.firstChild.firstChild is not None):
                self._on_create(xml_id.firstChild.firstChild.data)


class Job(HpcObject):
    """Represents Windows HPC job"""
    
    def __init__(self, server, id=0):
        HpcObject.__init__(self, server)
        self.id = id
        
    def _get_url(self):
        """Gets job's URL (/Job/{Id})"""
        return "Job/"+str(self.id)
    
    def _get_create_url(self):
        """Gets URL used for job creation (/Jobs)"""
        return "Jobs"
        
    def _on_create(self, response):
        """Handles extracted server response on job creation"""
        self.id = int(response)
                
    def submit(self, run_as_user=None, run_as_pwd=None):
        """Submits job, which is already created on the server"""
        
        # Create request elements with RunAs user name
        # and password if provided
        request_elements = dict()
        
        if run_as_user is not None:
            request_elements['UserName'] = run_as_user
            if run_as_pwd is not None:
                request_elements['Password'] = run_as_pwd
        
        # Submit by POSTing to /Job/{Id}/Submit
        submit_url = self._get_url() + "/Submit"
        response = self._server.send_properties("POST", submit_url, request_elements)
        
        
    def cancel(self, message=None, forceful=False):
        """Cancels job"""
        
        # URL for job cancelation is /Job/{Id}/Cancel
        cancel_url = self._get_url() + "/Cancel"
        
        # To cancel job forcefully 'Forced' query
        # string param is used
        if forceful:
            cancel_url += "?Forced=true"
        
        # Cancelation message can also be provided in request's body
        xml_cancel = '<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">' + str(message) + '</string>'
        
        # Send request via POST
        response = self._server.send_xml("POST", cancel_url, xml_cancel)
        
        
    def create_task(self):
        """Creates empty task within this job"""
        return Task(self)
        
 
       
class Task(HpcObject):
    """Represents Windows HPC task"""
    
    def __init__(self, parent_job, task_id=0):
        HpcObject.__init__(self, parent_job._server)
        self.parent_job = parent_job
        self.task_id = task_id
                
    def _get_url(self):
        """Gets task's URL ({JobURL}/Task/{TaskId})"""
        return self.parent_job._get_url()+"/Task/"+str(self.task_id)
    
    def _get_create_url(self):
        """Gets URL used for task creation ({JobURL}/Tasks)"""
        return self.parent_job._get_url()+"/Tasks"
        
    def _on_create(self, response):
        """Handles extracted server response on TaskCreation"""
        self.task_id = int(response)
