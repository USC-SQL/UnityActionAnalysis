from groundtruth import *

Move = TreeNode('56', [
    TreeEdge(ConditionLabel('Input.GetButtonDown("Left")', 'True'), TemplateNode('x')),
    TreeEdge(ConditionLabel('Input.GetButtonDown("Left")', 'False'),
             TreeNode('60', [
                 TreeEdge(ConditionLabel('Input.GetButtonDown("Right")', 'True'), TemplateNode('x')),
                 TreeEdge(ConditionLabel('Input.GetButtonDown("Right")', 'False'),
                          TreeNode('64', [
                              TreeEdge(ConditionLabel('Input.GetButtonDown("Up")', 'True'), TemplateNode('x')),
                              TreeEdge(ConditionLabel('Input.GetButtonDown("Up")', 'False'),
                                       TreeNode('68', [
                                           TreeEdge(ConditionLabel('Input.GetButtonDown("Down")', 'True'), TemplateNode('x')),
                                           TreeEdge(ConditionLabel('Input.GetButtonDown("Down")', 'False'),
                                                    TreeNode('72', [
                                                        TreeEdge(ConditionLabel('Input.GetButtonDown("Reset")', 'True'), TemplateNode('x')),
                                                        TreeEdge(ConditionLabel('Input.GetButtonDown("Reset")', 'False'),
                                                                 TreeNode('74', [
                                                                     TreeEdge(ConditionLabel('Input.GetButtonDown("Quit")', 'True'), TemplateNode('x')),
                                                                     TreeEdge(ConditionLabel('Input.GetButtonDown("Quit")', 'False'), TemplateNode('x'))
                                                                 ]))
                                                    ]))
                                       ]))
                          ]))
             ]))
])

tree = substitute(Move, 'x', EndNode())
print_conditions(unique_conditions(to_conditions(tree)))