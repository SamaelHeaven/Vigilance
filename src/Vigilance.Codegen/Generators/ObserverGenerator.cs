using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class ObserverGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            #pragma warning disable CS9084

            namespace Vigilance.Core;
            """
        );
        for (var i = 0; i < 16; i++)
        {
            var typeParams = $"<{string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"))}>";
            var typeArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n} t{n}"));
            var typeRefArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"ref T{n} t{n}"));
            var args = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"t{n}"));
            var refArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"ref t{n}"));
            sb.AppendLine(ComponentObserver(typeParams, args, refArgs, typeArgs, typeRefArgs));
        }
    }

    private static string ComponentObserver(string typeParams,string args,string refArgs, string typeArgs, string typeRefArgs)
    {
        return $$"""

            public unsafe struct ComponentObserver{{typeParams}}
            {
                private Scene _scene;
                private ulong _eventId;
                private Inclusion _disabled;
                private Inclusion _parent;
                private InlineIds _with;
                private InlineIds _without;
                
                internal ComponentObserver(Scene scene, ulong eventId) 
                {
                    _disabled = Inclusion.Include;
                    _parent = Inclusion.Include;
                    _scene = scene;
                    _eventId = eventId;
                }
                
                public ref ComponentObserver{{typeParams}} Disabled(Inclusion disabled) 
                {
                    _disabled = disabled;
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} Parent(Inclusion parent) 
                {
                    _parent = parent;
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} With(in Component component) 
                {
                    _with.Add(component.Id);
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} With<T>() 
                {
                    _with.Add(Flecs.NET.Core.Type<T>.Id(_scene.World));
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} With(in InlineIds with) 
                {
                    _with = with;
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} Without(in Component component) 
                {
                    _without.Add(component.Id);
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} Without<T>() 
                {
                    _without.Add(Flecs.NET.Core.Type<T>.Id(_scene.World));
                    return ref this;
                }
                
                public ref ComponentObserver{{typeParams}} Without(in InlineIds without) 
                {
                    _without = without;
                    return ref this;
                }
                
                public void Each(EachAction action) 
                {
                    var scene = _scene;
                    Observer().Each({{Each(typeRefArgs, $"action.Invoke({args})")}});
                }
                
                public void Each(EachEntityAction action) 
                {
                    var scene = _scene;
                    Observer().Each({{Each(typeRefArgs, $"action.Invoke(new Entity(it.Handle->entities[i], scene), {args})")}});
               }
               
                public void Each(EachRefAction action) 
                {
                    var scene = _scene;
                    Observer().Each({{Each(typeRefArgs, $"action.Invoke({refArgs})")}});
                }
              
                public void Each(EachEntityRefAction action) 
                {
                    var scene = _scene;
                    Observer().Each({{Each(typeRefArgs, $"action.Invoke(new Entity(it.Handle->entities[i], scene), {refArgs})")}});
                }
                
                public delegate void EachAction({{typeArgs}});
                
                public delegate void EachEntityAction(Entity entity, {{typeArgs}});
                
                public delegate void EachRefAction({{typeRefArgs}});
                
                public delegate void EachEntityRefAction(Entity entity, {{typeRefArgs}});
                
                private Flecs.NET.Core.ObserverBuilder{{typeParams}} Observer() 
                {
                    var observer = _scene.World.Observer{{typeParams}}().Event(_eventId);
                    switch (_disabled) 
                    {
                        case Inclusion.Include:
                            observer.With(Flecs.NET.Core.Ecs.Disabled).Filter().Optional();
                            break;
                        case Inclusion.Exclude:
                            observer.Without(Flecs.NET.Core.Ecs.Disabled);
                            break;
                        case Inclusion.Only:
                            observer.With(Flecs.NET.Core.Ecs.Disabled).Filter();
                            break;
                    }
                    
                    switch (_parent) 
                    {
                        case Inclusion.Exclude:
                            observer.Without(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard);
                            break;
                        case Inclusion.Only:
                            observer.With(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard).Filter();
                            break;
                    }
            
                    foreach (var id in _with) 
                        observer.With(id).Filter();
                    foreach (var id in _without) 
                        observer.Without(id);
                    return observer;
                }

            }
            """;
    }
    
    private static string Each(string typeRefArgs, string invoke)
    {
        return $$"""
            
                        (Flecs.NET.Core.Iter it, int i, {{typeRefArgs}}) =>
                        {
                            try 
                            {    
                                scene.DeferredCount++;
                                {{invoke}};
                            }
                            finally
                            {
                                if (scene.DeferredCount > 0)
                                    scene.DeferredCount--;
                            }
                        }
            """;
    }
}
