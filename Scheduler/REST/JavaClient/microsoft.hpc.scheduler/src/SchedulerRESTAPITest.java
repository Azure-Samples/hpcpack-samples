// Copyright � Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

import static org.junit.Assert.*;

import java.io.IOException;

import microsoft.hpc.scheduler.*;

import org.junit.Test;

/**
 * JUnit tests for Java Scheduler REST API
 * @author japrom
 *
 */
public class SchedulerRESTAPITest {
	public String _userName = "admin";
	public String _password = "!!123abc";
	public String _clusterHostname = "hpcjapromnus.cloudapp.net";

	/**
	 * Opens job with jobid 1 and asserts that we created an object with jobid1
	 */
	@Test
	public void testJobOpen()
	{
		IScheduler s = new Scheduler(_userName, _password, true);
		try
		{
			s.connect(_clusterHostname);
			ISchedulerJob job = s.openJob(1);
			assertEquals(1, job.getId());
		}
		catch(Exception e)
		{
			e.printStackTrace();
			fail("Unknown exception caught");
		}
	}
	
	
	/**
	 * Creates a job, sets the job name, and submits. Need manual validation that it actually submitted..
	 */
	@Test
	public void testCreateJob()
	{
		IScheduler s = new Scheduler(_userName, _password, true);
		try
		{
			s.connect(_clusterHostname);
			ISchedulerJob job = s.createJob();
			assertTrue(job.getId() > 1);
			job.setName("Java Job");
			s.SubmitJob(job, _userName, _password);
			assertTrue("Manually verify", true);
		}catch(Exception e)
		{
			e.printStackTrace();
			fail("unknown exception caught");
		}
	}
	
	/**
	 * Creates a job, submits a job, and set some job properties (progress and progress message are the easiest)
	 */
	@Test
	public void testPutJobProperties()
	{
		IScheduler s = new Scheduler(_userName, _password, true);
		try
		{
			s.connect(_clusterHostname);
			ISchedulerJob job = s.createJob();
			
			job.setRunUntilCanceled(true);
			job.setAutoCalculateMax(false);
			job.setAutoCalculateMin(false);
			
			job.setMinimumNumberOfCores(1);
			job.setMaximumNumberOfCores(1);
			
			s.SubmitJob(job, _userName, _password);
			
			Thread.sleep(5 * 1000); //Sleep for 5 seconds, give the scheduler time to react
			
			job.setProgress(50);
			job.setProgressMessage("Java job");
			job.commit();
			assertTrue("Manually verify", true);
		}
		catch(Exception e)
		{
			e.printStackTrace();
			fail("unknown exception caught");
		}
	}
}
