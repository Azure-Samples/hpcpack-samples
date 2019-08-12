# python2.7, python3

import sys, json

data = json.load(sys.stdin)
processStartInfo = data['m_Item2']
commandLine = processStartInfo.get('commandLine')
mpiSource = processStartInfo['environmentVariables'].get('CCP_MPI_SOURCE')
if commandLine and mpiSource:
    if mpiSource.endswith('/mpirun') or mpiSource.endswith('/mpiexec'):
        mpiCommand = '/'.join(mpiSource.split('/')[:-1]) + '/mpiexec'
    elif mpiSource.endswith('/'):
        mpiCommand = '{}mpiexec'.format(mpiSource)
    elif mpiSource.endswith('/mpivars.sh'):
        mpiCommand = 'source {}; mpiexec'.format(mpiSource)
    else:
        mpiCommand = '{}/mpiexec'.format(mpiSource)
    mpiCommand += ' '
    if 'CCP_MPI_HOSTFILE_FORMAT' in processStartInfo['environmentVariables']:
        mpiCommand += '-machinefile $CCP_MPI_HOSTFILE '
    processStartInfo['commandLine'] = commandLine.replace('mpiexec ', mpiCommand).replace('mpirun ', mpiCommand)

print(json.dumps(data))