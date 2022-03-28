from countDistinctStates import countDistinctStates
import os.path
import sys
import xml.etree.ElementTree as ET

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

def shouldSkipClass(className):
    return className.startswith('UnitySymexCrawler.') or className.startswith('Tiny.') or className.startswith('UnityStateDumper.')

def computeLineCoverage(opencovPath):
    xmlPath = os.path.join(opencovPath, "Recording", "RecordingCoverageResults_0000.xml")
    tree = ET.parse(xmlPath)
    root = tree.getroot()
    classes = root.findall('.//*/Class')
    totalSequencePoints = 0
    visitedSequencePoints = 0
    for cls in classes:
        className = cls.find('./FullName').text
        if shouldSkipClass(className):
            continue
        summary = cls.find('./Summary')
        totalSequencePoints += int(summary.attrib['numSequencePoints'])
        visitedSequencePoints += int(summary.attrib['visitedSequencePoints'])
    return float(visitedSequencePoints)/float(totalSequencePoints)

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
        row = [configName]
        for statecov in statecovs:
            row.append(str(statecov))
        for linecov in linecovs:
            row.append(str(linecov))
        outFile.write(','.join(row))
        outFile.write('\n')
