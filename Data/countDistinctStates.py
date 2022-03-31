import json
import sys
import hashlib
import os.path
import shutil

def defaultHash(s):
    js = json.dumps(s, sort_keys=True)
    return hashlib.sha256(js.encode('utf-8')).hexdigest()

def uniqueGameObjectsHash(s):
    gameObjects = set()
    def traverseGameObject(go):
        gameObject = go["gameObject"]
        goId = '{}:{}'.format(gameObject["name"], gameObject["tag"])
        gameObjects.add(goId)
        for child in go["children"]:
            traverseGameObject(child)
    for scn in s["scenes"]:
        for go in scn["rootGameObjects"]:
            traverseGameObject(go)
    return hash(frozenset(gameObjects))

def countDistinctStates(dumps, maxTime, stateHashFn):
    covered = set()
    i = 0
    for d in dumps:
        filename = os.path.basename(d)
        t = float(os.path.splitext(filename)[0].split('-')[1])
        if t <= maxTime:
            with open(d, 'r') as f:
                s = json.loads(f.read())
                h = stateHashFn(s)
                covered.add(h)
        i += 1
    return len(covered)

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
