from groundtruth import *

RotateRight = TreeNode('156',
                       [
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.G)', 'True'),
                                    TemplateNode('x')),
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.G)', 'False'),
                                    TemplateNode('x'))
                       ])

RotateLeft = TreeNode('169',
                       [
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.F)', 'True'),
                                    TemplateNode('x')),
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.F)', 'False'),
                                    TemplateNode('x'))
                       ])

MoveLeft = TreeNode('182',
                       [
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.LeftArrow)', 'True'),
                                    TemplateNode('x')),
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.LeftArrow)', 'False'),
                                    TemplateNode('x'))
                       ])

MoveRight = TreeNode('195',
                       [
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.RightArrow)', 'True'),
                                    TemplateNode('x')),
                           TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.RightArrow)', 'False'),
                                    TemplateNode('x'))
                       ])

MoveDown = TreeNode('209',
                       [
                           TreeEdge(ConditionLabel('Input.GetKey(KeyCode.DownArrow)', 'True'),
                                    TemplateNode('x')),
                           TreeEdge(ConditionLabel('Input.GetKey(KeyCode.DownArrow)', 'False'),
                                    TemplateNode('x'))
                       ])

tree = substitute(RotateRight, 'x', substitute(RotateLeft, 'x', substitute(MoveLeft, 'x', substitute(MoveRight, 'x', substitute(MoveDown, 'x', EndNode())))))
print_conditions(unique_conditions(to_conditions(tree)))