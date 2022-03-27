from groundtruth import *

Shoot = TreeNode('ShipShooter:27', [
    TreeEdge(ConditionLabel('Input.GetButtonDown("Fire1")', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetButtonDown("Fire1")', 'False'), TemplateNode('x'))
])

Hyperspacing = TreeNode('ShipMovement:32', [
    TreeEdge(ConditionLabel('Input.GetButtonDown("Fire2")', 'True'), EndNode()),
    TreeEdge(ConditionLabel('Input.GetButtonDown("Fire2")', 'False'), TemplateNode('x'))
])

Turn = TreeNode('ShipMovement:33', [
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '-1'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '0'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '1'), TemplateNode('x'))
])

Forward = TreeNode('ShipMovement:34', [
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '-1'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '0'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetAxis("Vertical")', '1'), TemplateNode('x'))
])

tree1 = substitute(Hyperspacing, 'x', substitute(Turn, 'x', substitute(Forward, 'x', EndNode())))
tree2 = substitute(Shoot, 'x', EndNode())
print_conditions(unique_conditions(to_conditions(tree1) + to_conditions(tree2)))