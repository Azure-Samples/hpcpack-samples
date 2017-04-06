#Overwrite the default config file so that the test lmstat output is used"
copy-item flexlm.exe.config -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe.config'
copy-item lmstat-output.txt -Destination 'C:\lmstat-output.txt'

# copy the files the Scheduler will pass to the Activation FIlter for different jobs requesting various license combinations
copy-item nolicense.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\nolicense.xml'
copy-item sixfslicenses.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml'
copy-item SixfsandSixfs_fracturereservoirLicenses.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\SixfsandSixfs_fracturereservoirLicenses.xml'
copy-item twentysixfslicenses.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\twentysixfslicenses.xml'
copy-item invalidlicense.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\invalidlicense.xml'
copy-item growsixfs.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\growsixfs.xml'
copy-item growstarfs_fracturereservoir.xml -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\growstarfs_fracturereservoir.xml'

# Activation Filter return codes
$StartJob = 0
$DontRunHoldQueue = 1
$DontRunKeepResourcesAllowOtherJobsToSchedule = 2
$HoldJobReleaseResourcesAllowOtherJobsToSchedule = 3
$FailJob = 4
$RejectAdditionofResources = 1

write-host test 1: If Scheduler pass is 1 then cache file should be deleted

& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\nolicense.xml' 1 1 true 22
if (test-path -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml')
{
	write-host "failed to delete cache"
	exit 1
}


write-host test 2: If backfill and licenses set on the job then return DontRunKeepResourcesAllowOtherJobsToSchedule

& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 2 true 6
if ($LastExitCode -ne $DontRunKeepResourcesAllowOtherJobsToSchedule)
{
	write-host "failed, LastExitCode is $LastExitCode"
	exit 1
}

write-host test 3: If backfill and licenses not set on the job then return StartJob

& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\nolicense.xml' 1 2 true 22
if ($LastExitCode -ne $StartJob)
{
	write-host "failed, LastExitCode is $LastExitCode"
	exit 1
}

write-host test 4: Request a series of jobs all requiring licenses. All jobs in a single Scheduler pass

# delete the cache to start the test cleanly
if (test-path -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml')
{
	remove-item -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml'
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 1 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 4.1 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.1.xml'
write-host starting 4.2
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 2 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 4.2 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.2.xml'
write-host starting 4.3
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 3 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 4.3 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.3.xml'
write-host starting 4.4
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 4 false 6
if ($LastExitCode -ne $DontRunKeepResourcesAllowOtherJobsToSchedule)
{
	write-host "test 4.4 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.4.xml'
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 5 false 6
if ($LastExitCode -ne $DontRunKeepResourcesAllowOtherJobsToSchedule)
{
	write-host "test 4.5 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.5.xml'
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\nolicense.xml' 1 6 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 4.6 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.6.xml'
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\growsixfs.xml' 1 6 false 6
if ($LastExitCode -ne $RejectAdditionofResources)
{
	write-host "test 4.7 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.7.xml'
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\growstarfs_fracturereservoir.xml' 1 6 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 4.8 failed, LastExitCode is $LastExitCode"
	exit 1
}
copy-item 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml' -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm4.8.xml'

write-host test 5: License jobs fail if FlexLM server down

# Copy a config file that does not return any licenses
copy-item flexlmdown.exe.config -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.exe.config'
# delete the cache to start the test cleanly
if (test-path -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml')
{
	remove-item -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml'
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 2 1 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 5.1 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\nolicense.xml' 2 2 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 5.2 failed, LastExitCode is $LastExitCode"
	exit 1
}
#Overwrite the default config file so that the test lmstat output is used"
copy-item flexlm.exe.config -Destination 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.exe.config'

write-host test 6: Reservations should expire

# delete the cache to start the test cleanly
if (test-path -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml')
{
	remove-item -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml'
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 1 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 6.1 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 2 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 6.2 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 3 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 6.3 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 1 4 false 6
if ($LastExitCode -ne $DontRunKeepResourcesAllowOtherJobsToSchedule)
{
	write-host "test 6.4 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 2 1 false 6
if ($LastExitCode -ne $DontRunKeepResourcesAllowOtherJobsToSchedule)
{
	write-host "test 6.5 failed, LastExitCode is $LastExitCode"
	exit 1
}
write-host "sleeping.."
Start-Sleep -s 30
write-host "woken up"
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\sixfslicenses.xml' 3 1 false 6
if ($LastExitCode -ne $StartJob)
{
	write-host "test 6.4 failed, LastExitCode is $LastExitCode"
	exit 1
}

write-host test 7: Request more licenses than allocated, request invalid license

# delete the cache to start the test cleanly
if (test-path -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml')
{
	remove-item -Path 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\flexlm.xml'
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\twentysixfslicenses.xml' 1 1 false 26
if ($LastExitCode -ne $FailJob)
{
	write-host "test 7.1 failed, LastExitCode is $LastExitCode"
	exit 1
}
& 'C:\Program Files\Microsoft HPC Pack 2008 R2\bin\flexlm.exe' 'C:\Program Files\Microsoft HPC Pack 2008 R2\Data\invalidlicense.xml' 1 2 false 6
if ($LastExitCode -ne $FailJob)
{
	write-host "test 7.2 failed, LastExitCode is $LastExitCode"
	exit 1
}


write-host "All tests passed"
exit 0