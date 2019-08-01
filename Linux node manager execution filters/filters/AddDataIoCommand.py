# python2.7, python3

import sys, json

j = json.load(sys.stdin)

HpcDataClient = '/opt/HpcData/HpcDataClient.exe'
commandLine = j['m_Item2'].get('commandLine')
if commandLine:
    inputFiles = j['m_Item2'].get('inputFiles')
    outputFiles = j['m_Item2'].get('outputFiles')
    if outputFiles and not outputFiles.isspace():
        commandUpload = '{} upload /source:. /dest:{} /overwrite'.format(HpcDataClient, outputFiles)
        commandLine = '({}); ec=$? && {} || exit 192 && exit $ec'.format(commandLine, commandUpload)
    if inputFiles and not inputFiles.isspace():
        commandDownload = '{} download /source:{} /dest:. /overwrite'.format(HpcDataClient, inputFiles)
        commandLine = '{} || exit 191 && {}'.format(commandDownload, commandLine)
    j['m_Item2']['commandLine'] = commandLine

print(json.dumps(j))