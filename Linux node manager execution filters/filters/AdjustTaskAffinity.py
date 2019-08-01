# python2.7, python3

import sys, json, subprocess, math

j = json.load(sys.stdin)

numaInfo = subprocess.check_output('lscpu | grep NUMA', shell = True)
numaCoreId = []
for line in numaInfo.splitlines():
    if line.startswith('NUMA') and 'CPU(s):' in line:
        coreIds = line.split()[-1].split(',')
        for coreId in coreIds:
            if '-' in coreId:
                beginEnd = map(int, coreId.split('-'))
                numaCoreId += list(range(beginEnd[0], beginEnd[1] + 1))
            else:
                numaCoreId.append(int(coreId))

AFFINITY_BITS = 64
if numaCoreId:
    affinity = j['m_Item2'].get('affinity') # This is an array of signed 64 bit number, which will be converted to bit array format for adjusting core id.
    if affinity:
        affinityList = [bit for int64 in affinity for bit in list('{:064b}'.format((2 ** AFFINITY_BITS - 1) & int64))[::-1]]
        mappedCoreIds = set([numaCoreId[coreId] for coreId in range(len(affinityList)) if affinityList[coreId] == '1'])
        mappedAffinityList = ['1' if coreId in mappedCoreIds else '0' for coreId in range(int(math.ceil(float(len(numaCoreId)) / AFFINITY_BITS) * AFFINITY_BITS))]
        j['m_Item2']['affinity'] = [int(''.join(mappedAffinityList[i * AFFINITY_BITS : (i + 1) * AFFINITY_BITS - 1][::-1]), 2) - int(mappedAffinityList[(i + 1) * AFFINITY_BITS - 1]) * 2 ** (AFFINITY_BITS - 1) for i in range(len(mappedAffinityList) // AFFINITY_BITS)]

    ccpCoreIds = j['m_Item2']['environmentVariables'].get('CCP_COREIDS')
    if ccpCoreIds:
        j['m_Item2']['environmentVariables']['CCP_COREIDS'] = ' '.join([str(numaCoreId[originalCoreId]) for originalCoreId in map(int, ccpCoreIds.split())])

print(json.dumps(j))