package microsoft.hpc.scheduler;

import java.util.Date;

/**
 * ISchedulerTask interface for Java HPC REST client. (dev note: You may need to change/update some return types) 
 * @author japrom
 *
 */
public interface ISchedulerTask {
	void commit();
	//Get Task Custom Properties
	INameValueCollection getCustomProperties();
	void refresh();
	void serviceConclude(Boolean cancelSubTasks);
	//Set Task Custom Properties
	void setCustomProperty(String name, String value);
	//Set Task Environment Variables
	void setEnvironmentVariable(String name, String value);
	String getAllocatedCoreIds();
	IStringCollection getAllocatedNodes();
	Date getChangeTime();
	String getCommandLine();
	void setCommandLine(String value);
	Date getCreateTime();
	IStringCollection getDependsOn();
	void setDependsOn(IStringCollection value);
	byte[] getEncryptedUserBlob();
	void setEncryptedUserBlob(byte[] value);
	Date getEndTime();
	int getEndValue();
	void setEndValue(int value);
	//Get Task Environment Variables
	INameValueCollection getEnvironmentVariables();
	void setEnvironmentVariables(INameValueCollection value);
	//Set Task Properties
	String getErrorMessage();
	int getExitCode();
	Boolean getHasRuntime(Boolean value);
	int getIncrementValue();
	void setIncrementValue(Boolean value);
	Boolean getIsExclusive();
	void setIsExclusive(Boolean value);
	Boolean getIsParametric();
	void setIsParametric(Boolean value);
	Boolean getIsRerunnable();
	void setIsRerunnable(Boolean value);
	Boolean getIsServiceConcluded();
	int getMaximumNumberOfCores();
	void setMaximumNumberOfCores(int value);
	int getMaximumNumberOfNodes();
	void setMaximumNumberOfNodes(int value);
	int getMaximumNumberOfSockets();
	void setMaximumNumberOfSockets(int value);
	int getMinimumNumberOfCores();
	void setMinimumNumberOfCores(int value);
	int getMinimumNumberOfNodes();
	void setMinimumNumberOfNodes(int value);
	int getMinimumNumberOfSockets();
	void setMinimumNumberOfSockets(int value);
	String getName();
	void setName(String value);
	String getOutput();
	int getParentJobId();
	String getPreviousState();
	int getRequeueCount();
	IStringCollection getRequiredNodes();
	void setRequiredNodes(IStringCollection value);
	int getRuntime();
	void setRuntime(int value);
	Date getStartTime();
	int getStartValue();
	void setStartValue(int value);
	String getState();
	String getStdErrFilePath();
	void setStdErrFilePath(String value);
	String getStdInFilePath();
	void setStdInFilePath(String value);
	String getStdOutFilePath();
	void setStdOutFilePath(String value);
	Date getSubmitTime();
	void setSubmitTime(Date value);
	int getTaskId();
	String getType();
	void setType(String value);
	String getUserBlob();
	void setUserBlob();
	String getWorkDirectory();
	void setWorkDirectory(String value);
	
	//
	//REST API specific
	//
	//Cancel Subtask
	void cancelSubtask(int subTaskId, String message, Boolean forced);
	//Cancel Subtask
	void cancelSubtask(int subTaskId, String message);
	//Cancel Subtask
	void cancelSubtask(int subTaskId);
	//Get Subtask
	ISchedulerTask getSubtask(int subTaskId);
	//Requeue Subtask
	void requeueSubTask(int subTaskId);
	//Set Subtask Properties
	//void setSubTaskProperties() ?
	
	//
	// Public APIs that have no REST equivalent
	//
	//ISchedulerTaskCounters GetCounters()
}
