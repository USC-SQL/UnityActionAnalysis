from groundtruth import *

Horiz = TreeNode('PlayerController:110,111', [
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '-1'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '0'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '1'), TemplateNode('x'))
])

Vert = TreeNode('PlayerController:110,111', [
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '-1'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '0'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '1'), TemplateNode('x'))
])

Pause = TreeNode('GameGUINavigation:42', [
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.Escape)', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.Escape)', 'False'), TemplateNode('x'))
])

tree1 = substitute(Horiz, 'x', substitute(Vert,'x',  EndNode()))
tree2 = substitute(Pause, 'x', EndNode())

print_conditions(unique_conditions(to_conditions(tree1) + to_conditions(tree2)))