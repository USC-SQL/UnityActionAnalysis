from groundtruth import *

DMS = TreeNode('Input.GetKeyDown(KeyCode.Z)',
         TreeEdge('T', TemplateNode('x')),
         TreeEdge('F', TreeNode('Input.GetKeyUp(KeyCode.Z)',
                                    TreeEdge('T', TemplateNode('x')),
                                    TreeEdge('F', TemplateNode('x')))))

Jump = TreeNode('Input.GetKeyDown(KeyCode.X)',
                TreeEdge('T', TemplateNode('x')),
                TreeEdge('F', TemplateNode('x')))

Move = TreeNode('Input.GetKey(KeyCode.LeftArrow)',
                TreeEdge('T', TemplateNode('x')),
                TreeEdge('F', TreeNode('Input.GetKey(KeyCode.RightArrow)',
                                       TreeEdge('T', TemplateNode('x')),
                                       TreeEdge('F', TemplateNode('x')))))

result = substitute(DMS, 'x', substitute(Jump, 'x', substitute(Move, 'x', EndNode())))
print_conditions(to_conditions(result))