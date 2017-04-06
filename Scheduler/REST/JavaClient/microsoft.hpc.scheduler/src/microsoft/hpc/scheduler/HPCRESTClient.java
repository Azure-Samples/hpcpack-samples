package microsoft.hpc.scheduler;

import java.io.*;

import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import java.security.SecureRandom;
import java.security.cert.CertificateException;

import javax.net.ssl.*;
import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.Unmarshaller;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.stream.StreamSource;

import org.w3c.dom.Document;
import org.xml.sax.SAXException;

import com.microsoft.schemas.hpcs2008r2.common.ArrayOfProperty;

import java.security.cert.X509Certificate;
import java.util.Hashtable;

/***
 * HPC RESTClient for JAVA
 * @author japrom
 *
 */
abstract class HPCRESTClient {
	
	private TrustManager[] _trustAllManager = null;
	
	/**
	 * Returns a TrustManager for SSL that does no validation and allows all certificates
	 * @return a all-trusting TrustManager
	 */
	private TrustManager[] getTrustAllManager()
	{
		if(_trustAllManager == null)
		{
			_trustAllManager = new TrustManager[] {
					new X509TrustManager() {
						public X509Certificate[] getAcceptedIssuers() {
							return null;
						}

						@Override
						public void checkClientTrusted(
								X509Certificate[] arg0,
								String arg1) throws CertificateException {
							return;
						}

						@Override
						public void checkServerTrusted(
								X509Certificate[] arg0,
								String arg1) throws CertificateException {
							return;
						}
					}
			};
		}
		return _trustAllManager;
	}
	
	private Hashtable<String, String> _customHeaders = null;
	
	private void HandleHttpError(HttpsURLConnection connection) throws IOException
	{
		if(connection.getResponseCode() >= 400) //Errors start at 400
		{
			BufferedReader reader = new BufferedReader(
					new InputStreamReader(connection.getErrorStream())
					);
			String line = null;
			while((line = reader.readLine()) != null)
			{
				System.err.println(line);
			}
		}
	}
	
	protected Hashtable<String, String> getCustomHeaders()
	{
		if(_customHeaders == null)
		{
			_customHeaders = new Hashtable<>();
		}
		return _customHeaders;
	}
	
	/**
	 * Constructor for HPCRestClient. Adds basic auth, and whether or not the client should be trusting of all servers (disregard ssl cert)
	 * @param username
	 * @param password
	 * @param trustAllCerts
	 */
	protected HPCRESTClient(String username, String password, Boolean trustAllCerts)
	{
		//
		// Add username/password to the header
		//
		if(username != null && !username.isEmpty() &&
				password != null && !password.isEmpty())
		{
			String userNameAndPassword = String.format("%s:%s", username, password);
			String base64Encoded = Base64.encode(userNameAndPassword);
			String headerValue = String.format("Basic %s", base64Encoded);
			
			if(_customHeaders == null)
				_customHeaders = new Hashtable<>();
				
			_customHeaders.put("Authorization", headerValue);
		}
		
		if(trustAllCerts)
		{
			try
			{
				SSLContext sslContext = SSLContext.getInstance("SSL");
				sslContext.init(null, getTrustAllManager(), new SecureRandom());
				HttpsURLConnection.setDefaultSSLSocketFactory(sslContext.getSocketFactory());
				
				HttpsURLConnection.setDefaultHostnameVerifier(new HostnameVerifier(){
					@Override
					public boolean verify(String arg0, SSLSession arg1) {
						return true;
					}
					
				});
			} catch (Exception e)
			{
				;
			}
		}
		else
		{
			SSLSocketFactory sslSocketFactory = (SSLSocketFactory)SSLSocketFactory.getDefault();
			HttpsURLConnection.setDefaultSSLSocketFactory(sslSocketFactory);
		}
	}
	
	/**
	 * Generates a GET method request
	 * @param address
	 * @return
	 * @throws IOException
	 */
	protected InputStream getRequest(String address) throws IOException
	{
		URL requestURL = null;;
		HttpsURLConnection connection = null;
		try {
			
			requestURL = new URL(address);
		
			connection = (HttpsURLConnection)requestURL.openConnection();
			connection.setRequestMethod("GET");
			 
		//We are handling the URL and Protocols internally, so none of these exceptions SHOULD be thrown
		} catch (MalformedURLException e) {
			e.printStackTrace();
		} catch (ProtocolException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		if(connection == null)
			return null;
		
		//
		// Add headers to the connection
		//
		if(_customHeaders != null)
		{
			for (String header : _customHeaders.keySet())
			{
				connection.setRequestProperty(header, _customHeaders.get(header));
			}
		}
		
		try
		{
			return connection.getInputStream();
		}
		catch(IOException e)
		{
			HandleHttpError(connection);
			throw e;
		}
	}

	/**
	 * Generates a POST method request
	 * @param address
	 * @param data
	 * @return
	 * @throws IOException
	 */
	protected InputStream postRequest(String address, String data) throws IOException
	{
		return sendRequest(address, data, "POST");
	}
	
	/**
	 * Generates a PUT method request
	 * @param address
	 * @param data
	 * @return
	 * @throws IOException
	 */
	protected InputStream putRequest(String address, String data) throws IOException
	{
		return sendRequest(address, data, "PUT");
	}
	
	/**
	 * Generates a request that sends data (used by both POST and PUT)
	 * @param address
	 * @param data
	 * @param method
	 * @return
	 * @throws IOException
	 */
	protected InputStream sendRequest(String address, String data, String method) throws IOException
	{
		URL requestURL = null;;
		HttpsURLConnection connection = null;
		try {
			
			requestURL = new URL(address);
		
			connection = (HttpsURLConnection)requestURL.openConnection();
			connection.setRequestMethod(method);
			connection.setDoOutput(true);
			connection.setDoInput(true);
			connection.setUseCaches(false);
			
			connection.setRequestProperty ( "Content-Type", "text/xml" );
			 
		//We are handling the URL and Protocols internally, so none of these exceptions SHOULD be thrown
		} catch (MalformedURLException e) {
			e.printStackTrace();
		} catch (ProtocolException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		if(connection == null)
			return null;
		
		//
		// Add headers to the connection
		//
		if(_customHeaders != null)
		{
			for (String header : _customHeaders.keySet())
			{
				connection.setRequestProperty(header, _customHeaders.get(header));
			}
		}
		
		//
		// Add data to request
		//
		OutputStreamWriter writer = new OutputStreamWriter(connection.getOutputStream());
		writer.write(data);
		writer.flush();
		
		try
		{
			return connection.getInputStream();
		}
		catch(IOException e)
		{
			HandleHttpError(connection);
			throw e;
		}
	}
	
	/**
	 * Returns a serialized version of Array of Property
	 * @param property
	 * @return
	 */
	protected String serializeArrayOfProperty(ArrayOfProperty property)
	{
		//
		// Serialize from ArrayOfProperty to xml (as String)
		//
		StringWriter writer = new StringWriter();
		
		try {
			
			JAXBContext jc = JAXBContext.newInstance(ArrayOfProperty.class);
			Marshaller u = jc.createMarshaller();
			u.marshal(property, writer);
			String data = writer.toString();
			//Do we need to be able to change the namespace? Some have updated namespace strings....
			//data = data.replaceAll("http://schemas.microsoft.com/HPCS2008R2/common", "http://schemas.datacontract.org/2004/07/SchedulerWebService");

			return data;
			
		} catch (JAXBException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return null;
		}
	}
	
	/**
	 * Returns a deserialized version of Array of Property
	 * @param xmlStream
	 * @return
	 */
	protected ArrayOfProperty deserializeArrayOfProperty(InputStream xmlStream)
	{
		try
		{
			StreamSource source = new StreamSource(xmlStream);
			//
			// Deserialize to object of ArrayOfProperty
			//
			JAXBContext jc = JAXBContext.newInstance(ArrayOfProperty.class);
			Unmarshaller u = jc.createUnmarshaller();
			ArrayOfProperty arrayOfProperty = (ArrayOfProperty) u.unmarshal(source);
			return arrayOfProperty;
			
		} catch (JAXBException e ) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return null;
	}
	
	/**
	 * Returns a deserialized Int (doesn't actually do any serializing, just extracts the int from XML)
	 * @param xmlStream
	 * @return
	 */
	protected int deserializeInt(InputStream xmlStream)
	{
		return Integer.parseInt(deserializeString(xmlStream));
	}
	
	/**
	 * Return a deserialized String (doesn't actually do anything deserializing, just extracts the string from XML)
	 * @param xmlStream
	 * @return
	 */
	protected String deserializeString(InputStream xmlStream)
	{
		try {
			//For debugging purposes, print the xmlStream to stdout
			/*BufferedReader rd = new BufferedReader(new InputStreamReader(xmlStream));
	        String line;
	        while ((line = rd.readLine()) != null) {
	            System.out.println(line);
	        }*/
	        
			//
			// Easier to just read the xml than try to deserialize it
			//
			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
			DocumentBuilder docBuilder = factory.newDocumentBuilder();
			Document doc = docBuilder.parse(xmlStream);
			doc.getDocumentElement().normalize();
			
			String value = doc.getFirstChild().getTextContent();
			return value;
			
		} catch (SAXException | IOException | ParserConfigurationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return null;
		}
	}
}
