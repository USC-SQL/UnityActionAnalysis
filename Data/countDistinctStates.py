import json
import sys
import hashlib
import os.path
import shutil

class StateCoverageAccum:
    def __init__(self, stateHashFn):
        self.covered = set()
        self.stateHashFn = stateHashFn

    def addState(self, dumpPath):
        with open(dumpPath, 'r') as f:
            s = json.load(f)
            h = self.stateHashFn(s)
            self.covered.add(h)

    def getStateCoverage(self):
        return len(self.covered)

def defaultHash(s):
    def transformGameObject(go):
        return {
            'gameObject': go['gameObject'],
            'children': list(map(transformGameObject, go['children']))
        }
    t = dict()
    scenes = t['scenes'] = list()
    for scn in s['scenes']:
        scnt = {
            'name': scn['name'],
            'rootGameObjects': list(map(transformGameObject, scn['rootGameObjects']))
        }
        scenes.append(scnt)
    js = json.dumps(t, sort_keys=True)
    return hashlib.sha256(js.encode('utf-8')).hexdigest()

def uniqueGameObjectsHash(s):
    gameObjects = set()
    def gameObjectKey(go):
        key = set()
        key.add(go['gameObject']['tag'])
        for componentTypeName in go['components'].keys():
            key.add(componentTypeName)
        return frozenset(key)
    def traverseGameObject(go):
        gameObjects.add(gameObjectKey(go))
        for child in go["children"]:
            traverseGameObject(child)
    for scn in s["scenes"]:
        for go in scn["rootGameObjects"]:
            traverseGameObject(go)
    return hash(frozenset(gameObjects))

def countDistinctStates(dumps, maxTime, stateHashFn):
    accum = StateCoverageAccum(stateHashFn)
    for d in dumps:
        filename = os.path.basename(d)
        t = float(os.path.splitext(filename)[0].split('-')[1])
        if t <= maxTime:
            accum.addState(d)
    return accum.getStateCoverage()

if __name__ == '__main__':
    dumpDir = sys.argv[1]
    maxTime = float('inf')
    if len(sys.argv) > 2:
        maxTime = float(sys.argv[2])
    dumps = [os.path.join(dumpDir, f) for f in os.listdir(dumpDir) if f.endswith('.json')]
    numDistinctStates = countDistinctStates(dumps, maxTime, defaultHash)
    numUniqueGoDistinctStates = countDistinctStates(dumps, maxTime, uniqueGameObjectsHash)
    print('{} distinct states considering all game objects'.format(numDistinctStates))
    print('{} distinct states considering unique game objects'.format(numUniqueGoDistinctStates))
