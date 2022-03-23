from collections import namedtuple

TreeNode = namedtuple('TreeNode', ['label', 'left_edge', 'right_edge'])
TreeEdge = namedtuple('TreeEdge', ['label', 'node'])
TemplateNode = namedtuple('TemplateNode', ['name'])
EndNode = namedtuple('EndNode', [])

def substitute(root, template_name, subtree):
    if isinstance(root, TemplateNode) and root.name == template_name:
        return subtree
    elif isinstance(root, TreeNode):
        return TreeNode(root.label,
                    TreeEdge(root.left_edge.label, substitute(root.left_edge.node, template_name, subtree)),
                    TreeEdge(root.right_edge.label, substitute(root.right_edge.node, template_name, subtree)))
    else:
        return root

def node_label(node):
    if isinstance(node, TreeNode):
        return node.label
    elif isinstance(node, EndNode):
        return 'end'
    else:
        return str(node)

def to_dotty(root):
    edges = set()
    def traverse(root, path):
        if isinstance(root, TreeNode):
            edges.add((node_label(root) + ' ' + str(hash(str(path))), node_label(root.left_edge.node) + ' ' + str(hash(str(path + [root, 'l']))), root.left_edge.label))
            edges.add((node_label(root) + ' ' + str(hash(str(path))), node_label(root.right_edge.node) + ' ' + str(hash(str(path + [root, 'r']))), root.right_edge.label))
            traverse(root.left_edge.node, path + [root, 'l'])
            traverse(root.right_edge.node, path + [root, 'r'])
    traverse(root, [])
    s = 'digraph tree {\n'
    for edge in edges:
        s += '    "{}"->"{}" [label="{}"]'.format(edge[0], edge[1], edge[2]) + '\n'
    s += '}\n'
    return s

def should_include_condition(cond):
    for p in cond:
        for q in cond:
            if p[1] == q[1] and p[0] != q[0]:
                return False
    return True

def to_conditions(tree):
    conditions = dict()
    def process_edge(root, edge, condition):
        if edge.label == 'T':
            cond = True
        elif edge.label == 'F':
            cond = False
        else:
            raise Exception('unexpected edge label {}'.format(edge.label))
        traverse(edge.node, condition + [(cond, root.label)])
    def traverse(root, condition):
        if isinstance(root, TreeNode):
            process_edge(root, root.left_edge, condition)
            process_edge(root, root.right_edge, condition)
        elif isinstance(root, EndNode):
            cond = set(condition)
            if should_include_condition(cond):
                conditions[str(cond)] = cond
        else:
            raise Exception('unexpected node {}'.format(root))
    traverse(tree, [])
    return list(conditions.values())

def print_conditions(conditions):
    for cond in conditions:
        print(cond)