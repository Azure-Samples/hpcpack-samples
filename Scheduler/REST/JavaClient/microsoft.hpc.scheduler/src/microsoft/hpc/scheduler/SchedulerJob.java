package microsoft.hpc.scheduler;

import java.io.IOException;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

import javax.xml.stream.XMLStreamReader;

import com.microsoft.schemas.hpcs2008r2.common.ArrayOfProperty;

/**
 * SchedulerJob implementation for Java HPC REST client. (dev note: You may need to change/update some return types) 
 * @author japrom
 *
 */
public class SchedulerJob implements ISchedulerJob {

	//This namevaluecollection contains committed properties
	INameValueCollection _jobProperties = null;
	//This namevaluecollection contains uncommitted properties
	INameValueCollection _unSubmitted = null;
	
	int _internalId = -1; //Having this in the arrayofproperty throws 400 Bad request
	Scheduler _scheduler = null;
	
	/**
	 * Generates a new, property-less ISchedulerJob
	 * @param s
	 */
	SchedulerJob(Scheduler s)
	{
		this(new NameValueCollection(), s);
	}
	
	/**
	 * Generates a new ISchedulerJob with (committed) jobProperties and a scheduler object (used to call REST APIs from this object) 
	 * @param jobProperties
	 * @param s
	 */
	SchedulerJob(INameValueCollection jobProperties, Scheduler s)
	{
		_jobProperties = jobProperties;
		_scheduler = s;
		_unSubmitted = new NameValueCollection();
		
		//
		// Remove ID from name value collection so it doesn't interfere with set job properties/submit
		//
		if(_jobProperties.containsKey("Id"))
		{
			_internalId = Integer.parseInt(_jobProperties.get("Id"));
			_jobProperties.remove("Id");
		}
	}
	
	/**
	 * Gets a value by first checking the uncommitted properties, then committed properties
	 * @param key
	 * @return
	 */
	String get(String key)
	{
		if(_unSubmitted.containsKey(key))
			return _unSubmitted.get(key);
		if(_jobProperties.containsKey(key))
			return _jobProperties.get(key);
		return null;
	}
	
	/**
	 * Stores the property into the unsubmitted property array. Use cleanupMergeUnsubmitted to (internally) commit the property
	 * @param name
	 * @param value
	 */
	void set(String name, Object value)
	{
		_unSubmitted.put(name,  value.toString());
	}
	
	/**
	 * Stores the property into the unsubmitted property array as a comma-delimited list
	 * @param name
	 * @param value
	 */
	void set(String name, IStringCollection value)
	{
		int num = value.size();
		if(num == 0)
			set(name, "");
		
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < num; i++)
		{
			if(i != 0)
				sb.append(";");
			
			sb.append(value.get(i));
		}
		
		set(name, sb.toString());
	}
	
	@Override
	public void addExcludedNodes(IStringCollection nodeNames) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void addTask(ISchedulerTask task) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void addTasks(ISchedulerTask[] taskList) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelTask(int taskId, String message, Boolean isForce) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelTask(int taskId, String message) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void cancelTask(int taskId) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void clearExcludedNodes() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void clearHold() {
		// TODO Auto-generated method stub
		
	}

	/**
	 * Commits the job properties to the scheduler using REST API: Put Job Properties
	 */
	@Override
	public void commit() throws IOException {
		_scheduler.commitJobProperties(this);
	}

	@Override
	public ISchedulerTask createTask() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void finish() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public INameValueCollection getCustomProperties() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public ISchedulerCollection getTaskIdList(Object filter,
			Object sort, Boolean expandParametric) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public ISchedulerCollection getTaskList(IFilterCollection filter,
			ISortCollection sort, Boolean expandParametric) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public ISchedulerTask openTask(int taskId) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void refresh() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void removeExcludedNodes(IStringCollection nodeNames) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void requeueTask(int taskId) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void restoreFromXml(XMLStreamReader reader) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void restoreFromXml(String url) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void setCustomProperty(String name, String value) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void setEnvironmentVariable(String name, String value) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void setHoldUntil(Date holdUntil) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void setJobTemplate(String templateName) {
		set("JobTemplate", templateName);
	}

	@Override
	public void submitTask(ISchedulerTask task) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void submitTaskById(int taskId) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void submitTasks(ISchedulerTask[] taskList) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public IStringCollection getAllocatedNodes() {
		return new StringCollection(get("AllocatedNodes"));
	}

	@Override
	public Boolean getAutoCalculateMax() {
		return Boolean.parseBoolean(get("AutoCalculateMax"));
	}

	@Override
	public void setAutoCalculateMax(Boolean value) {
		set("AutoCalculateMax", value);
	}

	@Override
	public Boolean getAutoCalculateMin() {
		return Boolean.parseBoolean(get("AutoCalculateMin"));
	}

	@Override
	public void setAutoCalculateMin(Boolean value) {
		set("AutoCalculateMin", value);
	}

	@Override
	public Boolean getCanGrow() {
		return Boolean.parseBoolean("CanGrow");
	}

	@Override
	public Boolean getCanPreempt() {
		return Boolean.parseBoolean(get("Preemptable"));
	}

	@Override
	public void setCanPreempt(Boolean value) {
		set("Preemptable", value);
	}

	@Override
	public Boolean getCanShrink() {
		return Boolean.parseBoolean("CanShrink");
	}

	@Override
	public Date getChangeTime() {
		DateFormat format = new SimpleDateFormat();
		try
		{
			return format.parse(get("ChangeTime"));
		}
		catch(ParseException e)
		{
			;
		}
		return null;
	}

	@Override
	public String getClientSource() {
		return get("ClientSource");
	}

	@Override
	public void setClientSource(String value) {
		set("ClientSource", value);
	}

	@Override
	public Date getCreateTime() {
		DateFormat format = new SimpleDateFormat();
		try
		{
			return format.parse(get("CreateTime"));
		}
		catch(ParseException e)
		{
			;
		}
		return null;
	}

	@Override
	public String getEmailAddress() {
		return get("EmailAddress");
	}

	@Override
	public void setEmailAddress(String value) {
		set("EmailAddress", value);
	}

	@Override
	public IStringCollection getEndpointAddresses() {
		return new StringCollection("EndpointReference");
	}

	@Override
	public Date getEndTime() {
		DateFormat format = new SimpleDateFormat();
		try
		{
			return format.parse(get("EndTime"));
		}
		catch(ParseException e)
		{
			;
		}
		return null;
	}

	@Override
	public INameValueCollection getEnvironmentVariables() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public String getErrorMessage() {
		return get("ErrorMessage");
	}

	@Override
	public IStringCollection getExcludedNodes() {
		return new StringCollection(get("ExcludedNodes"));
	}

	@Override
	public int getExpandedPriority() {
		return Integer.parseInt(get("ExpandedPriority"));
	}

	@Override
	public void setExpandedPriority(int value) {
		set("ExpandedPriority", value);
	}

	@Override
	public Boolean getFailOnTaskFailure() {
		return Boolean.parseBoolean(get("FailOnTaskFailure"));
	}

	@Override
	public void setFailOnTaskFailure(Boolean value) {		
		set("FailOnTaskFailure", value);
	}

	@Override
	public Boolean getHasRunTime() {
		return Boolean.parseBoolean(get("HasRuntime"));
	}

	@Override
	public Date getHoldUntil() {
		DateFormat format = new SimpleDateFormat();
		try
		{
			return format.parse(get("HoldUntil"));
		}
		catch(ParseException e)
		{
			;
		}
		return null;
	}

	@Override
	public int getId() {
		return _internalId;
	}

	@Override
	public Boolean getIsExclusive() {
		return Boolean.parseBoolean(get("IsExclusive"));
	}

	@Override
	public void setIsExclusive(Boolean value) {
		set("IsExclusive", value);
	}

	@Override
	public String getJobTemplate() {
		return get("JobTemplate");
	}

	@Override
	public int getMaxCoresPerNode() {
		return Integer.parseInt(get("MaxCoresPerNode"));
	}

	@Override
	public void setGetMaxCoresPerNode(int value) {
		set("MaxCoresPerNode", value);
	}

	@Override
	public int getMaximumNumberOfCores() {
		return Integer.parseInt(get("MaxCores"));
	}

	@Override
	public void setMaximumNumberOfCores(int value) {
		set("MaxCores", value);
	}

	@Override
	public int getMaximumNumberOfNodes() {
		return Integer.parseInt(get("MaxNodes"));
	}

	@Override
	public void setMaximumNumberOfNodes(int value) {
		set("MaxNodes", value);
	}

	@Override
	public int getMaximumNumberOfSockets() {
		return Integer.parseInt(get("MaxSockets"));
	}

	@Override
	public void setMaximumNumberOfSockets(int value) {
		set("MaxSockets", value);
	}

	@Override
	public int getMaxMemory() {
		return Integer.parseInt(get("MaxMemory"));
	}

	@Override
	public void setMaxMemory(int value) {
		set("MaxMemory", value);
	}
	
	@Override
	public int getMinMemory() {
		return Integer.parseInt(get("MinMemory"));
	}
	
	@Override
	public void setMinMemory(int value) {
		set("MinMemory", value);
	}

	@Override
	public int getMinCoresPerNode() {
		return Integer.parseInt(get("MinCoresPerNode"));
	}

	@Override
	public void setMinCoresPerNode(int value) {
		set("MinCoresPerNode", value);
	}

	@Override
	public int getMinimumNumberOfCores() {
		return Integer.parseInt(get("MinCores"));
	}

	@Override
	public void setMinimumNumberOfCores(int value) {
		set("MinCores", value);
	}

	@Override
	public int getMinimumNumberOfNodes() {
		return Integer.parseInt(get("MinNodes"));
	}

	@Override
	public void setMinimumNumberOfNodes(int value) {
		set("MinNodes", value);
		
	}

	@Override
	public int getMinimumNumberOfSockets() {
		return Integer.parseInt(get("MinSockets"));
	}

	@Override
	public void setMinimumNumberOfSockets(int value) {
		set("MinSockets", value);
	}

	@Override
	public String getName() {
		return get("Name");
	}

	@Override
	public void setName(String value) {
		set("Name", value);
	}

	@Override
	public IStringCollection getNodeGroups() {
		return new StringCollection(get("NodeGroups"));
	}

	@Override
	public void setNodeGroups(IStringCollection value) {
		set("NodeGroups", value);
	}

	@Override
	public Boolean getNotifyOnCompletion() {
		return Boolean.parseBoolean("NotifyOnCompletion");
	}

	@Override
	public void setNotifyOnCompletion(Boolean value) {
		set("NotifyOnCompletion", value);
	}

	@Override
	public Boolean getNotifyOnStart() {
		return Boolean.parseBoolean("NotifyOnStart");
	}

	@Override
	public void setNotifyOnStart(Boolean value) {
		set("NotifyOnStart", value);
	}

	@Override
	public String getOrderBy() {
		return get("OrderBy");
	}

	@Override
	public void setOrderBy(String value) {
		set("OrderBy", value);
	}

	@Override
	public String getOwner() {
		return get("Owner");
	}

	@Override
	public String getPool() {
		return get("Pool");
	}

	@Override
	public JobState getPreviousState() {
		return JobState.valueOf(get("PreviousState"));
	}

	@Override
	public JobPriority getPriority() {
		return JobPriority.valueOf(get("Priority"));
	}
	
	@Override
	public void setPriority(JobPriority value) {
		set("Priority", value);
	}

	@Override
	public int getProgress() {
		return Integer.parseInt(get("Progress"));
	}

	@Override
	public void setProgress(int value) {
		set("Progress", value);
	}

	@Override
	public String getProgressMessage() {
		return get("ProgressMessage");
	}

	@Override
	public void setProgressMessage(String value) {
		set("ProgressMessage", value);
	}

	@Override
	public String getProject() {
		return get("Project");
	}

	@Override
	public void setProject(String value) {
		set("Project", value);
	}

	@Override
	public IStringCollection getRequestedNodes() {
		return new StringCollection(get("RequestedNodes"));
	}

	@Override
	public void setRequestedNodes(IStringCollection value) {
		set("RequestedNodes", value);
	}

	@Override
	public int getRequeueCount() {
		return Integer.parseInt(get("RequeueCount"));
	}

	@Override
	public int getRuntime() {
		return Integer.parseInt(get("RuntimeSeconds"));
	}

	@Override
	public void setRunTime(int value) {
		set("RuntimeSeconds", value);
	}

	@Override
	public Boolean getRunUntilCanceled() {
		return Boolean.parseBoolean(get("RunUntilCanceled"));
	}

	@Override
	public void setRunUntilCanceled(Boolean value) {
		set("RunUntilCanceled", value);
	}

	@Override
	public String getServiceName() {
		return get("ServiceName");
	}

	@Override
	public void setServiceName(String value) {
		set("ServiceName", value);
	}

	@Override
	public IStringCollection getSoftwareLicense() {
		return new StringCollection(get("SoftwareLicense"));
	}

	@Override
	public void setSoftwareLicense(IStringCollection value) {
		set("SoftwareLicense", value);
	}

	@Override
	public Date getStartTime() {
		DateFormat format = new SimpleDateFormat();
		try {
			return format.parse(get("StartTime"));
		} catch (ParseException e) {
			;
		}
		return null;
	}

	@Override
	public JobState getState() {
		return JobState.valueOf(get("State"));
	}

	@Override
	public Date getSubmitTime() {
		DateFormat format = new SimpleDateFormat();
		try
		{
			return format.parse(get("SubmitTime"));
		}catch (ParseException e) {
			;
		}
		return null;
	}

	@Override
	public int getTargetResourceCount() {
		return Integer.parseInt(get("TargetResourceCount"));
	}

	@Override
	public void setTargetResourceCount(int value) {
		set("TargetResourceCount", value);
	}

	@Override
	public JobUnitType getUnitType() {
		String unitType = get("UnitType");
		return JobUnitType.valueOf(unitType);
	}

	@Override
	public void setUnitType(JobUnitType unitType) {
		set("UnitType", unitType);	
	}

	@Override
	public String getUserName() {
		return get("UserName");
	}

	@Override
	public void setUserName(String value) {
		set("UserName", value);
	}
	
	/***
	 * Returns the ArrayOfProperty version of this object
	 * @return
	 */
	ArrayOfProperty getArrayOfProperty()
	{
		return NameValueCollection.toArrayOfProperty(_unSubmitted);
	}

	/**
	 * Stores the password into this job object. Note that there's no corresponding "getPassword"
	 * @param value
	 */
	void setPassword(String value) {
		set("Password", value);
	}
	
	/**
	 * Stores the job id of this job object
	 * @param value
	 */
	void setId(int value) {
		_internalId = value;
	}
	
	/**
	 * Moves allt he properties from the unsubmitted array to the jobProperties array and clears unsubmitted
	 */
	void cleanupMergeUnsubmitted()
	{
		_jobProperties.putAll(_unSubmitted);
		_unSubmitted.clear();
	}
}
