from countDistinctStates import countDistinctStates
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

def computeLineCoverage(xmlPath):
    print(xmlPath)
    return 0

with open("{}.csv".format(os.path.basename(subjectDir)), 'w') as outFile:
    for configDir in configDirs:
        configName = os.path.basename(configDir)
        statecovs = []
        linecovs = []
        stateCovDir = os.path.join(configDir, 'statecov')
        lineCovDir = os.path.join(configDir, 'linecov')
        stateCovIterDirs = [os.path.join(stateCovDir, f)
            for f in os.listdir(stateCovDir)
            if os.path.isdir(os.path.join(stateCovDir, f))]
        lineCovIterDirs = [os.path.join(lineCovDir, f)
            for f in os.listdir(lineCovDir)
            if os.path.isdir(os.path.join(lineCovDir, f))]
        stateCovIterDirs.sort(key=lambda f: int(os.path.basename(f)))
        lineCovIterDirs.sort(key=lambda f: int(os.path.basename(f)))
        for stateCovIterDir in stateCovIterDirs:
            statecov = countDistinctStates(
                [os.path.join(stateCovIterDir, f) 
                    for f in os.listdir(stateCovIterDir)
                    if f.endswith('.json')], 
                float('inf'))
            statecovs.append(statecov)
        for lineCovIterDir in lineCovIterDirs:
            files = os.listdir(lineCovIterDir)
            if len(files) > 1:
                raise Exception('unexpected more than 1 file in {}'.format(lineCovIterDir))
            linecov = computeLineCoverage(os.path.join(lineCovIterDir, files[0]))
            linecovs.append(linecov)
        iterIndex = 1
        for statecov, linecov in zip(statecovs, linecovs):
            outFile.write('{},{},{},{}\n'.format(configName, iterIndex, statecov, linecov))
            iterIndex += 1
