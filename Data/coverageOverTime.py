from countDistinctStates import StateCoverageAccum, uniqueGameObjectsHash
from computeStmtCov import StatementCoverageAccum
import os.path
import sys

subjectDir = sys.argv[1]
codeCovSamplingRate = 2
timeInterval = 25
maxTime = 300

configs = os.listdir(subjectDir)
header = ['Time'] + configs

class RunCursor:
    def __init__(self, runId, stateCovIterDir, lineCovIterDir):
        self.runId = runId
        statedumps = [os.path.join(stateCovIterDir, f) for f in os.listdir(stateCovIterDir) if f.endswith('.json')]
        statedumps.sort(key=self.getStateDumpTime)
        self.statedumps = statedumps
        self.statedumpsPos = 0
        recordingDir = os.path.join(lineCovIterDir, os.listdir(lineCovIterDir)[0], 'Recording')
        codeCovSamples = [os.path.join(recordingDir, f) for f in os.listdir(recordingDir)]
        codeCovSamples.sort(key=self.getCodeCovSampleNum)
        self.codeCovSamples = codeCovSamples
        self.codeCovSamplesPos = 0
        self.stateCovAccum = StateCoverageAccum(uniqueGameObjectsHash)
        self.stmtCovAccum = StatementCoverageAccum()

    def getStateDumpTime(self, stateDumpPath):
        return float(os.path.splitext(os.path.basename(stateDumpPath))[0].split('-')[1])

    def getCodeCovSampleNum(self, samplePath):
        return int(os.path.splitext(os.path.basename(samplePath))[0].split('_')[1])

    def accumUntilTime(self, time):
        while self.statedumpsPos < len(self.statedumps) and self.getStateDumpTime(self.statedumps[self.statedumpsPos]) < time:
            self.stateCovAccum.addState(self.statedumps[self.statedumpsPos])
            self.statedumpsPos += 1
        while self.codeCovSamplesPos < len(self.codeCovSamples) and self.getCodeCovSampleNum(self.codeCovSamples[self.codeCovSamplesPos])*codeCovSamplingRate < time:
            self.stmtCovAccum.addSample(self.codeCovSamples[self.codeCovSamplesPos])
            self.codeCovSamplesPos += 1

    def getStateCoverage(self):
        return self.stateCovAccum.getStateCoverage()

    def getStmtCoverage(self):
        return self.stmtCovAccum.getStatementCoverage()

class ConfigCursor:
    def __init__(self, configName, configDir):
        self.configName = configName
        statecovDir = os.path.join(configDir, 'statecov')
        linecovDir = os.path.join(configDir, 'linecov')
        runIds = os.listdir(statecovDir)
        self.runCursors = dict()
        for runId in runIds:
            stateCovIterDir = os.path.join(statecovDir, runId)
            lineCovIterDir = os.path.join(linecovDir, runId)
            self.runCursors[runId] = RunCursor(runId, stateCovIterDir, lineCovIterDir)

    def accumUntilTime(self, time):
        for cursor in self.runCursors.values():
            cursor.accumUntilTime(time)

    def getAverageStateCoverage(self):
        avgStateCov = 0
        for cursor in self.runCursors.values():
            avgStateCov += cursor.getStateCoverage()
        return avgStateCov/len(self.runCursors)

    def getAverageStmtCoverage(self):
        avgStmtCov = 0
        for cursor in self.runCursors.values():
            avgStmtCov += cursor.getStmtCoverage()
        return avgStmtCov/len(self.runCursors)

configCursors = dict()
for config in os.listdir(subjectDir):
    configDir = os.path.join(subjectDir, config)
    cursor = ConfigCursor(config, configDir)
    configCursors[config] = cursor

configOrder = os.listdir(subjectDir)

stateCovPoints = []
stmtCovPoints = []
time = 0
while time <= maxTime:
    stateCovPoint = [time]
    stmtCovPoint = [time]
    print('Reading up to {} seconds...'.format(time))
    for config in configOrder:
        cursor = configCursors[config]
        cursor.accumUntilTime(time)
        stateCovPoint.append(cursor.getAverageStateCoverage())
        stmtCovPoint.append(cursor.getAverageStmtCoverage())
    stateCovPoints.append(stateCovPoint)
    stmtCovPoints.append(stmtCovPoint)
    time += timeInterval

stateCovOutFile = '{}.statecov.csv'.format(os.path.basename(subjectDir))
stmtCovOutFile = '{}.stmtcov.csv'.format(os.path.basename(subjectDir))

with open(stateCovOutFile, 'w') as f:
    f.write(','.join(['Time'] + configOrder))
    f.write('\n')
    for pt in stateCovPoints:
        f.write(','.join(map(str, pt)))
        f.write('\n')
with open(stmtCovOutFile, 'w') as f:
    f.write(','.join(['Time'] + configOrder))
    f.write('\n')
    for pt in stmtCovPoints:
        f.write(','.join(map(str, pt)))
        f.write('\n')
