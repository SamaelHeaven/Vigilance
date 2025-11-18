using System.Text;

namespace Vigilance.Codegen.Helpers;

public static class TraverserHelper
{
    public static void TraverserExtensions(
        this StringBuilder sb,
        string element,
        string traverser,
        string extensionPrefix = "",
        string asTraverserParams = "",
        string asTraverserArgs = ""
    )
    {
        sb.AppendLine(
            $$"""
                extension({{(extensionPrefix == "" ? "" : $"{extensionPrefix} ")}}{{element}} element)
                {
                    public {{traverser}} AsTraverser({{asTraverserParams}})
                    {
                        return new {{traverser}}(element{{(asTraverserArgs == "" ? "" : $", {asTraverserArgs}")}});
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{element}}>, {{element}}> Children()
                    {
                        return element.AsTraverser().Children();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{element}}>, {{element}}> ChildrenAndSelf()
                    {
                        return element.AsTraverser().ChildrenAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{element}}>, {{element}}> Descendants()
                    {
                        return element.AsTraverser().Descendants();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{element}}>, {{element}}> DescendantsAndSelf()
                    {
                        return element.AsTraverser().DescendantsAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{element}}>, {{element}}> Ancestors()
                    {
                        return element.AsTraverser().Ancestors();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{element}}>, {{element}}> AncestorsAndSelf()
                    {
                        return element.AsTraverser().AncestorsAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{element}}>, {{element}}> BeforeSelf()
                    {
                        return element.AsTraverser().BeforeSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{element}}>, {{element}}> BeforeSelfAndSelf()
                    {
                        return element.AsTraverser().BeforeSelfAndSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{element}}>, {{element}}> AfterSelf()
                    {
                        return element.AsTraverser().AfterSelf();
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{element}}>, {{element}}> AfterSelfAndSelf()
                    {
                        return element.AsTraverser().AfterSelfAndSelf();
                    }
                }
                
                extension({{traverser}} traverser)
                {
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{element}}>, {{element}}> Children()
                    {
                        return ZLinq.TraverserExtensions.Children<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Children<{{traverser}}, {{element}}>, {{element}}> ChildrenAndSelf()
                    {
                        return ZLinq.TraverserExtensions.ChildrenAndSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{element}}>, {{element}}> Descendants()
                    {
                        return ZLinq.TraverserExtensions.Descendants<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Descendants<{{traverser}}, {{element}}>, {{element}}> DescendantsAndSelf()
                    {
                        return ZLinq.TraverserExtensions.DescendantsAndSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{element}}>, {{element}}> Ancestors()
                    {
                        return ZLinq.TraverserExtensions.Ancestors<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.Ancestors<{{traverser}}, {{element}}>, {{element}}> AncestorsAndSelf()
                    {
                        return ZLinq.TraverserExtensions.AncestorsAndSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{element}}>, {{element}}> BeforeSelf()
                    {
                        return ZLinq.TraverserExtensions.BeforeSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.BeforeSelf<{{traverser}}, {{element}}>, {{element}}> BeforeSelfAndSelf()
                    {
                        return ZLinq.TraverserExtensions.BeforeSelfAndSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{element}}>, {{element}}> AfterSelf()
                    {
                        return ZLinq.TraverserExtensions.AfterSelf<{{traverser}}, {{element}}>(traverser);
                    }
                
                    public ZLinq.ValueEnumerable<ZLinq.Traversables.AfterSelf<{{traverser}}, {{element}}>, {{element}}> AfterSelfAndSelf()
                    {
                        return ZLinq.TraverserExtensions.AfterSelfAndSelf<{{traverser}}, {{element}}>(traverser);
                    }
                }

            """
        );
    }
}
