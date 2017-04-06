package microsoft.hpc.scheduler;

import java.io.IOException;
import java.net.MalformedURLException;

import javax.xml.stream.XMLStreamReader;

/**
 * IScheduler interface for Java HPC REST client. (dev note: You may need to change/update some return types) 
 * @author japrom
 *
 */
public interface IScheduler {
	void addJob(ISchedulerJob job);
	//Cancel Job
	void cancelJob(int id, String message, Boolean force);
	//Cancel Job
	void cancelJob(int id, String message);
	//Cancel Job
	void cancelJob(int id);
	ISchedulerJob cloneJob(int id);
	void close();
	void connect(String clusterName) throws MalformedURLException, IOException;
	//Create Job
	ISchedulerJob createJob() throws IOException;
	Object getJobIdList(Object filter, Object sort);
	//Get Job list
	Object getJobList(Object filter, Object sort);
	Object getJobTemplateInfo(String jobTemplateName);
	//Get Job Templates
	IStringCollection getJobTemplateList();
	XMLStreamReader getJobTemplateXML(String jobTemplateName);
	//Get Node Group List
	IStringCollection getNodeGroupList();
	Object getNodeIdList(Object filter, Object sort);
	//Get Node list
	Object getNodeList(Object filter, Object sort);
	//Get Node Group Members
	IStringCollection getNodesInNodeGroup(String nodeGroup);
	//Get Version
	Object getServerVersion();
	//Get Job
	ISchedulerJob openJob(int id) throws IOException;
	//Get Node
	ISchedulerNode openNode(int nodeId);
	//Get Node
	ISchedulerNode openNodeByName(String nodeName);
	void SetCachedCredentials(String userName, String password);
	void SetEnvironmentVariable(String name, String value);
	//Submit Job
	void SubmitJob(ISchedulerJob job, String username, String password) throws IOException;
	//Submit Job
	void SubmitJobById(int jobId, String username, String password);
	//Get Job Environment Variables
	INameValueCollection getEnvironmentVariables();
	
	//
	//REST API only APIs
	//
	//Requeue Job
	void requeueJob(int jobId);

	String getClientVersion();
	void setClientVersion(String value);
	
	//
	// Public APIs that have no REST equivalent
	//
	//void configureJob(int id);
	//IRemoteCommand createCommand(String commandLine, ICommandInfo info, IStringCollection nodes, Boolean redirectOutput);
	//IRemoteCommand createCommand(String commandLine, ICommandInfo info);
	//ICommandInfo createCommandInfo(INameValueCollection envs, String workDir, String stdIn);
	//IFilterCollection createFilterCollection()
	//IIntCollection createIntCollection()
	//INameValueCollection createNameValueCollection();
	//Properties.ITaskId createParametricTaskId(int jobTaskid, int instanceId);
	//ISchedulerPool createPool(String poolName, int poolWeight);
	//ISchedulerPool createPool(String poolName);
	//ISortCollection createSortCollection();
	//IStringCollection createStringCollection();
	//Properties.ITaskId createTaskId(int jobTaskId);
	//void deleteCachedCredentials(String userName);
	//void deletePool(String poolName, Boolean force);
	//void deletePool(String poolName);
	//String enrollCertificate(string templateName);
	//byte[] getCertificateFromStore(String thumbprint, out SecureString pfxPassword);
	//ISchedulerCounters getCounters();
	//ISchedulerRowEnumerator OpenJobEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort);
	//Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenJobHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
	//Microsoft.Hpc.Scheduler.ISchedulerRowSet OpenJobRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
	//ISchedulerRowEnumerator OpenNodeEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
	//ISchedulerRowEnumerator OpenNodeHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
	//ISchedulerPool OpenPool(string poolName)
	//ISchedulerRowSet OpenPoolRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties)
	//ISchedulerCollection getPoolList();
	//void SetCertificateCredentials(string userName, string thumbprint);
	//void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes);
	//void SetClusterParameter(string name, string value);
	//void SetInterfaceMode(bool isConsole, System.IntPtr hwnd);
	//Properties.UserPriviledge getUserPrivilege();
	//INameValueCollection getClusterParameters();
	//EventHandler OnSchedulerReconnect()
}
