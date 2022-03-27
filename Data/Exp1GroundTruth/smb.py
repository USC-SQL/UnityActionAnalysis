from groundtruth import *

FaceDirX = TreeNode('Mario:272',
    [ # this is technically Input.GetAxisRaw, but our implementation treats both as Input.GetAxis
        TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '-1'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '0'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetAxis("Horizontal")', '1'), TemplateNode('x'))
    ])

Dash = TreeNode('Mario:273',
    [
        TreeEdge(ConditionLabel('Input.GetButton("Dash")', 'True'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetButton("Dash")', 'False'), TemplateNode('x'))
    ])

Crouch = TreeNode('Mario:274',
    [
        TreeEdge(ConditionLabel('Input.GetButton("Crouch")', 'True'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetButton("Crouch")', 'False'), TemplateNode('x'))
    ])

Shoot = TreeNode('Mario:275',
    [
        TreeEdge(ConditionLabel('Input.GetButtonDown("Dash")', 'True'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetButtonDown("Dash")', 'False'), TemplateNode('x'))
    ])

Jump = TreeNode('Mario:276',
    [
        TreeEdge(ConditionLabel('Input.GetButton("Jump")', 'True'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetButton("Jump")', 'False'), TemplateNode('x'))
    ])

JumpRel = TreeNode('Mario:277',
    [
        TreeEdge(ConditionLabel('Input.GetButtonUp("Jump")', 'True'), TemplateNode('x')),
        TreeEdge(ConditionLabel('Input.GetButtonUp("Jump")', 'False'), TemplateNode('x'))
    ])

Pause = TreeNode('LevelManager:145',
     [
         TreeEdge(ConditionLabel('Input.GetButtonDown("Pause")', 'True'), TemplateNode('x')),
         TreeEdge(ConditionLabel('Input.GetButtonDown("Pause")', 'False'), TemplateNode('x'))
     ])

tree1 = substitute(FaceDirX, 'x', substitute(Dash, 'x', substitute(Crouch, 'x', substitute(Shoot, 'x', substitute(Jump, 'x', substitute(JumpRel, 'x', EndNode()))))))
tree2 = substitute(Pause, 'x', EndNode())

print_conditions(unique_conditions(to_conditions(tree1) + to_conditions(tree2)))
