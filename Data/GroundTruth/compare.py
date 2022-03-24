import sys
import json
from groundtruth import action_to_string

if len(sys.argv) < 3:
    sys.stderr.write('Usage: python {} <expected.json> <actual.json>\n'.format(sys.argv[0]))
    exit(1)

expected_path = sys.argv[1]
actual_path = sys.argv[2]

with open(expected_path, 'r') as f:
    expected_json = json.load(f)

with open(actual_path, 'r') as f:
    actual_json = json.load(f)

expected = set()
actual = set()

for action_json in expected_json:
    action = set()
    for cond in action_json:
        action.add((cond[0], cond[1]))
    expected.add(frozenset(action))

for action_json in actual_json:
    action = set()
    for cond in action_json:
        arr = cond.split(' == ')
        action.add((arr[0], arr[1]))
    actual.add(frozenset(action))

tp = set()
fp = set()
fn = set()

for action in expected:
    if action in actual:
        tp.add(action)
    else:
        fn.add(action)

for action in actual:
    if action not in expected:
        fp.add(action)

with open('expected.txt', 'w') as f:
    f.write('Expected:\n')
    expected_sorted = list(map(action_to_string, expected))
    expected_sorted.sort()
    f.write('\n'.join(expected_sorted))

with open('actual.txt', 'w') as f:
    f.write('Actual:\n')
    actual_sorted = list(map(action_to_string, actual))
    actual_sorted.sort()
    f.write('\n'.join(actual_sorted))
    
with open('fp.txt', 'w') as f:
    f.write('FP:\n')
    fp_sorted = list(map(action_to_string, fp))
    fp_sorted.sort()
    f.write('\n'.join(fp_sorted))

with open('fn.txt', 'w') as f:
    f.write('FN:\n')
    fn_sorted = list(map(action_to_string, fn))
    fn_sorted.sort()
    f.write('\n'.join(fn_sorted))

prec = len(tp)/(len(tp)+len(fp))
recall = len(tp)/(len(tp)+len(fn))

print('Precision = {}, Recall = {}'.format(prec, recall))