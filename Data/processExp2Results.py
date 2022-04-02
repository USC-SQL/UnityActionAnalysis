from countDistinctStates import countDistinctStates, uniqueGameObjectsHash, defaultHash
from computeStmtCov import computeStatementCoverage
import os.path
import sys
import json

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

headerCreated = False
with open("{}.csv".format(os.path.basename(subjectDir)), 'w') as outFile:
    for configDir in configDirs:
        configName = os.path.basename(configDir)
        numActionsPerformed = []
        statecovs = []
        statecovUniqueGos = []
        linecovs = []
        stateCovDir = os.path.join(configDir, 'statecov')
        lineCovDir = os.path.join(configDir, 'linecov')
        runIds = [f for f in os.listdir(stateCovDir) if os.path.isdir(os.path.join(stateCovDir, f))]

        for runId in runIds:
            stateCovIterDir = os.path.join(stateCovDir, runId)
            lineCovIterDir = os.path.join(lineCovDir, runId)
            statsFile = os.path.join(configDir, 'stats.{}.json'.format(runId))

            with open(statsFile, 'r') as f:
                stats = json.load(f)
                if 'NumActionsPerformed' in stats:
                    actionsPerformed = stats['NumActionsPerformed']
                else:
                    actionsPerformed = 0
                numActionsPerformed.append(actionsPerformed)

            statedumps = [os.path.join(stateCovIterDir, f) for f in os.listdir(stateCovIterDir) if f.endswith('.json')]
            statecov = countDistinctStates(statedumps, float('inf'), defaultHash)
            stateCovUniqueGo = countDistinctStates(statedumps, float('inf'), uniqueGameObjectsHash)
            statecovs.append(statecov)
            statecovUniqueGos.append(stateCovUniqueGo)

            files = os.listdir(lineCovIterDir)
            if len(files) > 1:
                raise Exception('unexpected more than 1 file in {}'.format(lineCovIterDir))
            linecov = computeLineCoverage(os.path.join(lineCovIterDir, files[0]))
            linecovs.append(linecov)

        if not headerCreated:
            header = ['Config']
            for i in range(len(numActionsPerformed)):
                header.append('Num Actions {}'.format(i))
            for i in range(len(statecovs)):
                header.append('State Cov {}'.format(i))
            for i in range(len(statecovUniqueGos)):
                header.append('State Cov (Unique Game Objects) {}'.format(i))
            for i in range(len(linecovs)):
                header.append('Stmt Cov {}'.format(i))
            outFile.write(','.join(header))
            outFile.write('\n')
            headerCreated = True

        row = [configName]
        for actionsPerformed in numActionsPerformed:
            row.append(str(actionsPerformed))
        for statecov in statecovs:
            row.append(str(statecov))
        for statecovUniqueGo in statecovUniqueGos:
            row.append(str(statecovUniqueGo))
        for linecov in linecovs:
            row.append(str(linecov))
        outFile.write(','.join(row))
        outFile.write('\n')
