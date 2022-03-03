import json
import sys
import hashlib
import os.path
import shutil

def countDistinctStates(dumps):
    covered = set()
    i = 0
    n = len(dumps)
    for d in dumps:
        filename = os.path.basename(d)
        with open(d, 'r') as f:
            s = json.loads(f.read())
            js = json.dumps(s, sort_keys=True)
            h = hashlib.sha256(js.encode('utf-8')).hexdigest()
            covered.add(h)
        i += 1
        print('{}/{} processed'.format(i, n))
    return len(covered)

if __name__ == '__main__':
    dumpDir = sys.argv[1]
    dumps = [os.path.join(dumpDir, f) for f in os.listdir(dumpDir) if f.endswith('.json')]
    num = countDistinctStates(dumps)
    print('{} distinct states'.format(num))
