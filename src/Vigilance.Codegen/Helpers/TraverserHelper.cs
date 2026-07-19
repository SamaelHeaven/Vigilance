using System.Text;

namespace Vigilance.Codegen.Helpers;

public static class TraverserHelper
{
    public static void TraverserExtensions(
        this StringBuilder sb,
        string origin,
        string traverser,
        string asTraverserParams = "",
        string asTraverserArgs = "",
        string typeParams = "",
        string typeConstraints = ""
    )
    {
        sb.AppendLine(
            $$"""
                extension{{typeParams}}({{origin}} origin) {{typeConstraints}}
                {
                    public {{traverser}} AsTraverser({{asTraverserParams}})
                    {
                        return new {{traverser}}(origin{{(asTraverserArgs == "" ? "" : $", {asTraverserArgs}")}});
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{origin}}>, {{origin}}> Children()
                    {
                        return origin.AsTraverser().Children();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{origin}}>, {{origin}}> ChildrenAndSelf()
                    {
                        return origin.AsTraverser().ChildrenAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> Descendants()
                    {
                        return origin.AsTraverser().Descendants();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsAndSelf()
                    {
                        return origin.AsTraverser().DescendantsAndSelf();
                    }
                    
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPreOrder()
                    {
                        return origin.AsTraverser().DescendantsPreOrder();
                    }
                    
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPreOrderAndSelf()
                    {
                        return origin.AsTraverser().DescendantsPreOrderAndSelf();
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsPostOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPostOrder()
                    {
                        return origin.AsTraverser().DescendantsPostOrder();
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsPostOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPostOrderAndSelf()
                    {
                        return origin.AsTraverser().DescendantsPostOrderAndSelf();
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsLevelOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsLevelOrder()
                    {
                        return origin.AsTraverser().DescendantsLevelOrder();
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsLevelOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsLevelOrderAndSelf()
                    {
                        return origin.AsTraverser().DescendantsLevelOrderAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{origin}}>, {{origin}}> Ancestors()
                    {
                        return origin.AsTraverser().Ancestors();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{origin}}>, {{origin}}> AncestorsAndSelf()
                    {
                        return origin.AsTraverser().AncestorsAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{origin}}>, {{origin}}> BeforeSelf()
                    {
                        return origin.AsTraverser().BeforeSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{origin}}>, {{origin}}> BeforeSelfAndSelf()
                    {
                        return origin.AsTraverser().BeforeSelfAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{origin}}>, {{origin}}> AfterSelf()
                    {
                        return origin.AsTraverser().AfterSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{origin}}>, {{origin}}> AfterSelfAndSelf()
                    {
                        return origin.AsTraverser().AfterSelfAndSelf();
                    }
                }
                
                extension{{typeParams}}({{traverser}} traverser) {{typeConstraints}}
                {
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{origin}}>, {{origin}}> Children()
                    {
                        return ZLinq.TraverserExtensions.Children<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{origin}}>, {{origin}}> ChildrenAndSelf()
                    {
                        return ZLinq.TraverserExtensions.ChildrenAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> Descendants()
                    {
                        return ZLinq.TraverserExtensions.Descendants<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsAndSelf()
                    {
                        return ZLinq.TraverserExtensions.DescendantsAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                    
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPreOrder()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsPreOrder<{{traverser}}, {{origin}}>(traverser);
                    }

                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPreOrderAndSelf()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsPreOrderAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsPostOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPostOrder()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsPostOrder<{{traverser}}, {{origin}}>(traverser);
                    }
                  
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsPostOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsPostOrderAndSelf()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsPostOrderAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsLevelOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsLevelOrder()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsLevelOrder<{{traverser}}, {{origin}}>(traverser);
                    }
                    
                    public ZLinq.ValueEnumerable<Vigilance.Collections.DescendantsLevelOrder<{{traverser}}, {{origin}}>, {{origin}}> DescendantsLevelOrderAndSelf()
                    {
                        return Vigilance.Collections.ZLinqExtensions.DescendantsLevelOrderAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{origin}}>, {{origin}}> Ancestors()
                    {
                        return ZLinq.TraverserExtensions.Ancestors<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{origin}}>, {{origin}}> AncestorsAndSelf()
                    {
                        return ZLinq.TraverserExtensions.AncestorsAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{origin}}>, {{origin}}> BeforeSelf()
                    {
                        return ZLinq.TraverserExtensions.BeforeSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{origin}}>, {{origin}}> BeforeSelfAndSelf()
                    {
                        return ZLinq.TraverserExtensions.BeforeSelfAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{origin}}>, {{origin}}> AfterSelf()
                    {
                        return ZLinq.TraverserExtensions.AfterSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{origin}}>, {{origin}}> AfterSelfAndSelf()
                    {
                        return ZLinq.TraverserExtensions.AfterSelfAndSelf<{{traverser}}, {{origin}}>(traverser);
                    }
                }

            """
        );
    }
}
