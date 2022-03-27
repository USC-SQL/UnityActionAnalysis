from collections import namedtuple
import json

TreeNode = namedtuple('TreeNode', ['label', 'edges'])
TreeEdge = namedtuple('TreeEdge', ['label', 'node'])
TemplateNode = namedtuple('TemplateNode', ['name'])
EndNode = namedtuple('EndNode', [])

ConditionLabel = namedtuple('ConditionLabel', ['variable', 'value'])

def substitute(root, template_name, subtree):
    if isinstance(root, TemplateNode) and root.name == template_name:
        return subtree
    elif isinstance(root, TreeNode):
        return TreeNode(root.label,
                        list(map(lambda edge: TreeEdge(edge.label, substitute(edge.node, template_name, subtree)), root.edges)))
    else:
        return root

def node_label(node):
    if isinstance(node, TreeNode):
        return node.label
    elif isinstance(node, EndNode):
        return 'end'
    else:
        return str(node)

def edge_label(edge):
    if isinstance(edge, ConditionLabel):
        return '{} == {}'.format(edge.variable, edge.value)
    else:
        return str(edge)

def to_dotty(root):
    edges = set()
    def traverse(root, path):
        if isinstance(root, TreeNode):
            for edge in root.edges:
                edges.add((node_label(root) + ' ' + str(hash(str(path))), node_label(edge.node) + ' ' + str(hash(str(path + [root, edge_label(edge.label)]))), edge_label(edge.label)))
            for edge in root.edges:
                traverse(edge.node, path + [root, edge_label(edge.label)])
    traverse(root, [])
    s = 'digraph tree {\n'
    for edge in edges:
        s += '    "{}"->"{}" [label="{}"]'.format(edge[0], edge[1], edge[2]) + '\n'
    s += '}\n'
    return s

def should_include_condition(cond):
    for p in cond:
        for q in cond:
            if p[0] == q[0] and p[1] != q[1]:
                return False
    return True

def to_conditions(tree):
    conditions = list()
    def traverse(root, condition):
        if isinstance(root, TreeNode):
            for edge in root.edges:
                if isinstance(edge.label, ConditionLabel):
                    traverse(edge.node, condition + [(edge.label.variable, edge.label.value)])
                else:
                    raise Exception("unexpected label {}".format(edge.label))
        elif isinstance(root, EndNode):
            cond = set(condition)
            if should_include_condition(cond):
                conditions.append(cond)
        else:
            raise Exception('unexpected node {}'.format(root))
    traverse(tree, [])
    return conditions

def unique_conditions(conds):
    res = dict()
    for cond in conds:
        res[str(cond)] = cond
    return list(res.values())

def print_conditions(conditions):
    res = []
    for cond in conditions:
        res.append(list(cond))
    print(json.dumps(res, indent=2))

def action_to_string(action):
    conds = list(map(lambda cond: '{} == {}'.format(cond[0], cond[1]), action))
    conds.sort()
    return ' && '.join(conds)