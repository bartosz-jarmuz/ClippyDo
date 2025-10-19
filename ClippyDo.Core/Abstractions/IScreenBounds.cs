namespace ClippyDo.Core.Abstractions;

public interface IScreenBounds { (double X, double Y, double Width, double Height) GetWorkAreaNearPointer(); }