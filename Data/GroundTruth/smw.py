from groundtruth import *

DMS = TreeNode('107',
               [
                   TreeEdge(ConditionLabel('Input.GetKey(KeyCode.Z)', 'True'),
                        TemplateNode('x')),
                   TreeEdge(ConditionLabel('Input.GetKey(KeyCode.Z)', 'False'),
                            TreeNode('119',
                                     [
                                        TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.Z)', 'True'),
                                              TemplateNode('x')),
                                        TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.Z)', 'False'),
                                              TemplateNode('x'))
                                     ]))
               ])

Jump = TreeNode('251',
                [
                    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.X)', 'True'),
                             TreeNode('261', [
                                 TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.X)', 'True'), TemplateNode('x')),
                                 TreeEdge(ConditionLabel('Input.GetKeyUp(KeyCode.X)', 'False'),
                                          TreeNode('266', [
                                              TreeEdge(ConditionLabel('Input.GetKey(KeyCode.X)', 'True'), TemplateNode('x')),
                                              TreeEdge(ConditionLabel('Input.GetKey(KeyCode.X)', 'False'), TemplateNode('x'))
                                          ]))
                             ])),
                    TreeEdge(ConditionLabel('Input.GetKeyDown(KeyCode.X)', 'False'),
                             TemplateNode('x'))
                ])

Move = TreeNode('67',
                [
                    TreeEdge(ConditionLabel('Input.GetKey(KeyCode.LeftArrow)', 'True'),
                             TemplateNode('x')),
                    TreeEdge(ConditionLabel('Input.GetKey(KeyCode.LeftArrow)', 'False'),
                             TreeNode('73',
                                      [
                                          TreeEdge(ConditionLabel('Input.GetKey(KeyCode.RightArrow)', 'True'),
                                                   TemplateNode('x')),
                                          TreeEdge(ConditionLabel('Input.GetKey(KeyCode.RightArrow)', 'False'),
                                                   TemplateNode('x'))
                                      ]))
                ])

tree = substitute(DMS, 'x', substitute(Jump, 'x', substitute(Move, 'x', EndNode())))
print_conditions(unique_conditions(to_conditions(tree)))