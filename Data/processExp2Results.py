from countDistinctStates import countDistinctStates, uniqueGameObjectsHash, defaultHash
from computeStmtCov import computeStatementCoverage
import os.path
import sys

if len(sys.argv) < 2:
    print("Usage: python {} <subjectDir>".format(sys.argv[0]))
    exit(1)

subjectDir = sys.argv[1]
sds = os.path.split(subjectDir)
if len(sds[1]) == 0:
    subjectDir = sds[0]

configDirs = [os.path.join(subjectDir, f) 
    for f in os.listdir(subjectDir)
    if os.path.isdir(os.path.join(subjectDir, f))]

def computeLineCoverage(opencovPath):
    recordingDir = os.path.join(opencovPath, 'Recording')
    return computeStatementCoverage([os.path.join(recordingDir, f) for f in os.listdir(recordingDir)])

with open("{}.csv".format(os.path.basename(subjectDir)), 'w') as outFile:
    for configDir in configDirs:
        configName = os.path.basename(configDir)
        statecovs = []
        statecovUniqueGos = []
        linecovs = []
        stateCovDir = os.path.join(configDir, 'statecov')
        lineCovDir = os.path.join(configDir, 'linecov')
        stateCovIterDirs = [os.path.join(stateCovDir, f) for f in os.listdir(stateCovDir) if os.path.isdir(os.path.join(stateCovDir, f))]
        lineCovIterDirs = [os.path.join(lineCovDir, f) for f in os.listdir(lineCovDir) if os.path.isdir(os.path.join(lineCovDir, f))]
        stateCovIterDirs.sort(key=lambda f: int(os.path.basename(f)))
        lineCovIterDirs.sort(key=lambda f: int(os.path.basename(f)))
        for stateCovIterDir in stateCovIterDirs:
            statedumps = [os.path.join(stateCovIterDir, f) for f in os.listdir(stateCovIterDir) if f.endswith('.json')]
            statecov = countDistinctStates(statedumps, float('inf'), defaultHash)
            stateCovUniqueGo = countDistinctStates(statedumps, float('inf'), uniqueGameObjectsHash)
            statecovs.append(statecov)
            statecovUniqueGos.append(stateCovUniqueGo)
        for lineCovIterDir in lineCovIterDirs:
            files = os.listdir(lineCovIterDir)
            if len(files) > 1:
                raise Exception('unexpected more than 1 file in {}'.format(lineCovIterDir))
            linecov = computeLineCoverage(os.path.join(lineCovIterDir, files[0]))
            linecovs.append(linecov)
        row = [configName]
        for statecov in statecovs:
            row.append(str(statecov))
        for statecovUniqueGo in statecovUniqueGos:
            row.append(str(statecovUniqueGo))
        for linecov in linecovs:
            row.append(str(linecov))
        outFile.write(','.join(row))
        outFile.write('\n')
