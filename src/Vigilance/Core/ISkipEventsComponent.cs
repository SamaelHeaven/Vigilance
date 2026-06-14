namespace Vigilance.Core;

public interface ISkipAddEventComponent;

public interface ISkipSetEventComponent;

public interface ISkipSetEventIfEqualComponent;

public interface ISkipRemoveEventComponent;

public interface ISkipEventsComponent : ISkipAddEventComponent, ISkipSetEventComponent, ISkipRemoveEventComponent;
