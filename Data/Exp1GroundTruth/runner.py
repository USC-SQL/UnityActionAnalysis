from groundtruth import *

Fire = TreeNode('PlayerHealthDamageShoot:35', [
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.K)', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.K)', 'False'), TemplateNode('x'))
])

DoubleJump = TreeNode('PlayerMovement:71', [
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.Space)', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.Space)', 'False'), TemplateNode('x'))
])

Jump = TreeNode('PlayerMovement:74', [
    TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.Space)', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.Space)', 'False'), TemplateNode('x'))
])

tree1 = substitute(DoubleJump, 'x', substitute(Jump, 'x', EndNode()))
tree2 = substitute(Fire, 'x', EndNode())
print_conditions(unique_conditions(to_conditions(tree1) + to_conditions(tree2)))