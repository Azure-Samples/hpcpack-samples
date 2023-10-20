# Introduction
This sample describes the process of filtering jobs by job id and job project name. Then, collect the job information including core duration or node duration.

# Usage
## Filter jobs by job id
- /id:<int>           Equal to the specific job id
- /id:<int>-<int>     Range of the job ids to consider

## Filter jobs by job project name
- /project:<string>   Only jobs with the specific Project are considered
- /projecti:<string>  Only jobs with the specific Project are considered, case insensitive
- /projectb:<string>  Only jobs that begin with the specific Project are considered
- /projectc:<string>  Only jobs that contain the specific Project are considered

## Collect job information
- /detailed           If present, list each core instead of summary of job use
- /nodes              If present, list each node (instead of the default core)
- Note: At least one item (Job id or range, user) must be declared
       /project and /projecti cannot be combined with other project parameters
       /projectb and /projectc can be combined together

## Examples
JobUse /id:45                  Core usage summary of Job 45
JobUse /id:45 /detailed        Core usage of each core used by Job 45
JobUse /id:45 /nodes           Node usage summary of Job 45
JobUse /id:45 /nodes /detailed Node usage of each node used by Job 45
JobUse /id:10-20               Core usage summary of each job from 10-20
JobUse /id:10-20 /project:G    Core usage summary of each G project job, 10-20
JobUse /id:10-20 /projectb:Red Core usage summary of jobs that begin with Red project within 10-20
JobUse /user:John              Core usage summary of each job John owns
JobUse /id:10-20 /user:John    Core usage summary of each job John owns, 10-20
