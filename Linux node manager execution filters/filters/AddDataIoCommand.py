# python2.7, python3

import sys, json

data = json.load(sys.stdin)
processStartInfo = data['m_Item2']
HpcDataClient = '/opt/HpcData/HpcDataClient.exe'
commandLine = processStartInfo.get('commandLine')
if commandLine:
    inputFiles = processStartInfo.get('inputFiles')
    outputFiles = processStartInfo.get('outputFiles')
    if outputFiles and not outputFiles.isspace():
        commandUpload = '{} upload /source:. /dest:{} /overwrite'.format(HpcDataClient, outputFiles)
        commandLine = '({}); ec=$? && {} || exit 192 && exit $ec'.format(commandLine, commandUpload)
    if inputFiles and not inputFiles.isspace():
        commandDownload = '{} download /source:{} /dest:. /overwrite'.format(HpcDataClient, inputFiles)
        commandLine = '{} || exit 191 && {}'.format(commandDownload, commandLine)
    processStartInfo['commandLine'] = commandLine

print(json.dumps(data))