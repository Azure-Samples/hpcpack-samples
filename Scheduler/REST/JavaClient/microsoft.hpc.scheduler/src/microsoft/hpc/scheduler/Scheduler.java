package microsoft.hpc.scheduler;

import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Hashtable;

import javax.xml.stream.XMLStreamReader;

import com.microsoft.schemas.hpcs2008r2.common.ArrayOfProperty;

/**
 * Scheduler implementation for Java HPC REST client. (dev note: You may need to change/update some return types) 
 * @author japrom
 *
 */
public class Scheduler extends HPCRESTClient implements IScheduler{
	
	private String _hostname = null;
	private String _baseUrlString = null;
	private final String _windowsHPC = "WindowsHPC";
	
	public Scheduler(String username, String password, Boolean trustAllCerts) {
		super(username, password, trustAllCerts);

		//
		// Set the default version
		//
		setClientVersion("2011-11-01");
	}
	
	/**
	 * Default constructor: No username/password (ntlm?), do not trust all certs
	 */
	public Scheduler()
	{
		this(null, null, false);
	}
	
	/**
	 * Username/password, do not trust all certs
	 * @param username
	 * @param password
	 */
	public Scheduler(String username, String password)
	{
		this(username, password, false);
	}

	@Override
	public void addJob(ISchedulerJob job) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelJob(int id, String message, Boolean force) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelJob(int id, String message) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelJob(int id) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public ISchedulerJob cloneJob(int id) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void close() {
		// TODO Auto-generated method stub
		
	}

	@Override
	/**
	 * Does not actually connect to anything (unless clusterName was not specified). Stores the Url and clustername (if not specified) internally.
	 */
	public void connect(String clusterUrl) throws IOException {
		URL cluster = null;
		if(clusterUrl.contains("://"))
		{	
			cluster = new URL(clusterUrl);
		}
		else
		{
			cluster = new URL(String.format("https://%s/", clusterUrl));
		}
		//
		// Parse base and clustername from URL (if possible)
		// 	Take the connectionURL and extract the hostname and clustername from the string
		//	This is done because some APIs (for example, Get Clusters) does not use the clusterName variable in the URL
		//
		
		_hostname = cluster.getHost();
		
		String path = cluster.getPath();

		String[] pathTokens = path.split("/");

		int tokenLen = pathTokens.length;
		if(tokenLen > 0)
		{
			//Very first token should be empty since path starts with "/"
			if(!pathTokens[0].isEmpty())
			{
				throw new MalformedURLException("Unexpected token in URL");
			}
			//Verify next token is WindowsHPC
			if(tokenLen > 1)
			{
				if(!pathTokens[1].equalsIgnoreCase(_windowsHPC))
					throw new MalformedURLException(String.format("Url does not contain %s", _windowsHPC));
			}
		}

		_baseUrlString = String.format("https://%s/%s/", _hostname, _windowsHPC);
	}
	

	/**
	 * Create a job and returns an ISchedulerJob object with the ID property set
	 */
	@Override
	public ISchedulerJob createJob() throws IOException {
		InputStream stream = postRequest(_baseUrlString + "Jobs", "");
		int id = deserializeInt(stream);
		SchedulerJob job = new SchedulerJob(this);
		job.setId(id);
		return job;
	}

	@Override
	public Object getJobIdList(Object filter, Object sort) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Object getJobList(Object filter, Object sort) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Object getJobTemplateInfo(String jobTemplateName) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public IStringCollection getJobTemplateList() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public XMLStreamReader getJobTemplateXML(String jobTemplateName) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public IStringCollection getNodeGroupList() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Object getNodeIdList(Object filter, Object sort) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Object getNodeList(Object filter, Object sort) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public IStringCollection getNodesInNodeGroup(String nodeGroup) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Object getServerVersion() {
		// TODO Auto-generated method stub
		return null;
	}

	/**
	 * Returns a ISchedulerJob object for a specified id
	 */
	@Override
	public ISchedulerJob openJob(int id) throws IOException {
		
		InputStream xmlStream = getRequest(_baseUrlString + "Job/" + id);
		
		ArrayOfProperty arrayOfProperty = deserializeArrayOfProperty(xmlStream);
		INameValueCollection nvCollection = (INameValueCollection) new NameValueCollection(arrayOfProperty);
		
		return new SchedulerJob(nvCollection, this);
	}

	@Override
	public ISchedulerNode openNode(int nodeId) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public ISchedulerNode openNodeByName(String nodeName) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void SetCachedCredentials(String userName, String password) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void SetEnvironmentVariable(String name, String value) {
		// TODO Auto-generated method stub
		
	}

	/**
	 * Submits a job as specified in the ISchedulerJob object. Must contain jobId
	 */
	@Override
	public void SubmitJob(ISchedulerJob job, String username, String password) throws IOException {
		//
		// Set the username and password of the job
		//
		job.setUserName(username);
		((SchedulerJob)job).setPassword(password);
		
		//Get the serialized version
		String data = serializeArrayOfProperty(((SchedulerJob)job).getArrayOfProperty());
		
		postRequest(_baseUrlString + String.format("Job/%d/Submit", job.getId()), data);
		((SchedulerJob) job).cleanupMergeUnsubmitted();
	}

	@Override
	public void SubmitJobById(int jobId, String username, String password) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public INameValueCollection getEnvironmentVariables() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void requeueJob(int jobId) {
		// TODO Auto-generated method stub
		
	}
	
	/**
	 * Returns the client version the API will use in the header
	 */
	@Override
	public String getClientVersion()
	{
		Hashtable<String, String> headers = getCustomHeaders();
		if(headers.containsKey("api-version"))
			return headers.get("api-version");
		else return null;
	}
	
	/**
	 * Sets the client version the API will use in the header
	 */
	@Override
	public void setClientVersion(String value)
	{
		getCustomHeaders().put("api-version", value);
	}
	
	/**
	 * Used by ISchedulerJob.Commit(). Calls the REST API: Put Job Properties
	 * @param job
	 * @throws IOException
	 */
	void commitJobProperties(ISchedulerJob job) throws IOException
	{
		String data = serializeArrayOfProperty(((SchedulerJob)job).getArrayOfProperty());
		int jobId = job.getId();
		putRequest(_baseUrlString + String.format("Job/%d", jobId), data);
		((SchedulerJob) job).cleanupMergeUnsubmitted();
	}

}
