#!/bin/bash

input=`cat`
job_id=`echo $input | grep -o '"JobId":[[:digit:]]*' | awk -F: '{print $NF}'`
task_id=`echo $input | grep -o '"TaskId":[[:digit:]]*' | awk -F: '{print $NF}'`
requeue_count=`echo $input | grep -o '"taskRequeueCount":[[:digit:]]*' | awk -F: '{print $NF}'`
log_dir=/opt/hpcnodemanager/filters/ERROR_LOG
mkdir -p $log_dir
log_prefix=$log_dir/$job_id.$task_id.$requeue_count
log_input=$log_prefix.input
log_error=$log_prefix.error

echo $input | (\
python /opt/hpcnodemanager/filters/AdjustTaskAffinity.py | \
python /opt/hpcnodemanager/filters/AdjustMpiCommand.py | \
python /opt/hpcnodemanager/filters/AddDataIoCommand.py \
) 2>$log_error

error_code=$?
[ ! -s $log_error ] && [ "$error_code" -eq "0" ] && rm -f $log_error || (echo $input >$log_input && exit $error_code)