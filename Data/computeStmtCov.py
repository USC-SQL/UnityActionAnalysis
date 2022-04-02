import os.path
import sys
import xml.etree.ElementTree as ET

class StatementCoverageAccum:
    def __init__(self):
        self.sequencePoints = {}

    def addSample(self, xmlPath):
        tree = ET.parse(xmlPath)
        root = tree.getroot()
        classes = root.findall('.//*/Class')
        for cls in classes:
            className = cls.find('./FullName').text
            if shouldSkipClass(className):
                continue
            methods = cls.findall('.//*/Method')
            for method in methods:
                metadataToken = method.find('./MetadataToken').text
                methodSeqPoints = method.findall('.//*/SequencePoint')
                for sp in methodSeqPoints:
                    spId = metadataToken + ':' + sp.attrib['offset']
                    visited = int(sp.attrib['vc']) > 0
                    if spId not in self.sequencePoints:
                        self.sequencePoints[spId] = visited
                    elif visited and not self.sequencePoints[spId]:
                        self.sequencePoints[spId] = True

    def getStatementCoverage(self):
        if len(self.sequencePoints) == 0:
            return 0
        numVisitedSeqPoints = 0
        for spId, visited in self.sequencePoints.items():
            if visited:
                numVisitedSeqPoints += 1
        return numVisitedSeqPoints / len(self.sequencePoints)

def shouldSkipClass(className):
    return className.startswith('UnitySymexCrawler.') or className.startswith('Tiny.') or className.startswith('UnityStateDumper.')

def computeStatementCoverage(samples):
    accum = StatementCoverageAccum()
    for xmlPath in samples:
        accum.addSample(xmlPath)
    return accum.getStatementCoverage()

if __name__ == '__main__':
    samplesDir = sys.argv[1]
    maxNum = None
    if len(sys.argv) > 2:
        maxNum = int(sys.argv[2])
    samples = []
    for f in os.listdir(samplesDir):
        if f.endswith('.xml'):
            sampleNum = int(os.path.splitext(f)[0].split('_')[1])
            if sampleNum <= maxNum:
                samples.append(os.path.join(samplesDir, f))
    print(computeStatementCoverage(samples))
