package microsoft.hpc.scheduler;

import java.io.IOException;
import java.util.Date;

import javax.xml.stream.XMLStreamReader;

/**
 * ISchedulerJob interface for Java HPC REST client. (dev note: You may need to change/update some return types) 
 * @author japrom
 *
 */
public interface ISchedulerJob {
	void addExcludedNodes(IStringCollection nodeNames);
	//Add Task
	void addTask(ISchedulerTask task);
	void addTasks(ISchedulerTask[] taskList);
	//Cancel Task
	void cancelTask(int taskId, String message, Boolean isForce);
	//Cancel Task
	void cancelTask(int taskId, String message);
	//Cancel Task
	void cancelTask(int taskId);
	void clearExcludedNodes();
	void clearHold();
	void commit() throws IOException;
	ISchedulerTask createTask();
	void finish();
	//Get Job Custom Properties
	INameValueCollection getCustomProperties();
	ISchedulerCollection getTaskIdList(Object filter, Object sort, Boolean expandParametric);
	//Get Task List
	ISchedulerCollection getTaskList(IFilterCollection filter, ISortCollection sort, Boolean expandParametric);
	//Get Task
	ISchedulerTask openTask(int taskId);
	void refresh();
	void removeExcludedNodes(IStringCollection nodeNames);
	//Requeue Task
	void requeueTask(int taskId);
	//Create Job From XML
	void restoreFromXml(XMLStreamReader reader);
	void restoreFromXml(String url);
	//Set Job Custom Properties
	void setCustomProperty(String name, String value);
	//Set Job Environment Variables
	void setEnvironmentVariable(String name, String value);
	//Set Job Properties
	void setHoldUntil(Date holdUntil);
	void setJobTemplate(String templateName);
	void submitTask(ISchedulerTask task);
	void submitTaskById(int taskId);
	void submitTasks(ISchedulerTask[] taskList);
	IStringCollection getAllocatedNodes();
	Boolean getAutoCalculateMax();
	void setAutoCalculateMax(Boolean value);
	Boolean getAutoCalculateMin();
	void setAutoCalculateMin(Boolean value);
	Boolean getCanGrow();
	Boolean getCanPreempt();
	void setCanPreempt(Boolean value);
	Boolean getCanShrink();
	Date getChangeTime();
	String getClientSource();
	void setClientSource(String value);
	Date getCreateTime();
	String getEmailAddress();
	void setEmailAddress(String value);
	IStringCollection getEndpointAddresses();
	Date getEndTime();
	//Get Job Environment Variables
	INameValueCollection getEnvironmentVariables();
	String getErrorMessage();
	IStringCollection getExcludedNodes();
	int getExpandedPriority();
	void setExpandedPriority(int value);
	Boolean getFailOnTaskFailure();
	void setFailOnTaskFailure(Boolean value);
	Boolean getHasRunTime();
	Date getHoldUntil();
	int getId();
	Boolean getIsExclusive();
	void setIsExclusive(Boolean value);
	String getJobTemplate();
	int getMaxCoresPerNode();
	void setGetMaxCoresPerNode(int value);
	int getMaximumNumberOfCores();
	void setMaximumNumberOfCores(int value);
	int getMaximumNumberOfNodes();
	void setMaximumNumberOfNodes(int value);
	int getMaximumNumberOfSockets();
	void setMaximumNumberOfSockets(int value);
	int getMaxMemory();
	void setMaxMemory(int value);
	int getMinMemory();
	void setMinMemory(int value);
	int getMinCoresPerNode();
	void setMinCoresPerNode(int value);
	int getMinimumNumberOfCores();
	void setMinimumNumberOfCores(int value);
	int getMinimumNumberOfNodes();
	void setMinimumNumberOfNodes(int value);
	int getMinimumNumberOfSockets();
	void setMinimumNumberOfSockets(int value);
	String getName();
	void setName(String value);
	IStringCollection getNodeGroups();
	void setNodeGroups(IStringCollection value);
	Boolean getNotifyOnCompletion();
	void setNotifyOnCompletion(Boolean value);
	Boolean getNotifyOnStart();
	void setNotifyOnStart(Boolean value);
	String getOrderBy();
	void setOrderBy(String value);
	String getOwner();
	String getPool();
	JobState getPreviousState();
	JobPriority getPriority();
	void setPriority(JobPriority value);
	int getProgress();
	void setProgress(int value);
	String getProgressMessage();
	void setProgressMessage(String value);
	String getProject();
	void setProject(String value);
	IStringCollection getRequestedNodes();
	void setRequestedNodes(IStringCollection value);
	int getRequeueCount();
	int getRuntime();
	void setRunTime(int value);
	Boolean getRunUntilCanceled();
	void setRunUntilCanceled(Boolean value);
	String getServiceName();
	void setServiceName(String value);
	IStringCollection getSoftwareLicense();
	void setSoftwareLicense(IStringCollection value);
	Date getStartTime();
	JobState getState();
	Date getSubmitTime();
	int getTargetResourceCount();
	void setTargetResourceCount(int value);
	JobUnitType getUnitType();
	void setUnitType(JobUnitType unitType);
	String getUserName();
	void setUserName(String value);
	
	//REST API Specific
	//(Made this internal. Use cast to access them in SchedulerJob. Good idea?. It might break some programming principles)
	//ArrayOfProperty getArrayOfProperty();
	//void setPassword(String value);
	
	//
	// These public APIs don't exist in REST 
	//
	//ISchedulerJobCounters GetCounters()
	//ISchedulerRowEnumerator OpenJobAllocationHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties)
	//ISchedulerRowEnumerator OpenTaskAllocationHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties)
	//ISchedulerRowEnumerator OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, bool expandParametric)
	//ISchedulerRowSet OpenTaskRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, bool expandParametric)
}
