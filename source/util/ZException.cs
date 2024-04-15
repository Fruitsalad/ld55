public class ZException : System.Exception {
    public ZException() {}
    public ZException(string message)
        : base(message) {}
    public ZException(string message, System.Exception inner)
        : base(message, inner) {}
}

public class InconsistencyException : System.Exception {
    public InconsistencyException() {}
    public InconsistencyException(string message)
        : base(message) {}
    public InconsistencyException(string message, System.Exception inner)
        : base(message, inner) {}
}

public class ShouldNotHappen : System.Exception {
    public ShouldNotHappen() {}
    public ShouldNotHappen(string message)
        : base(message) {}
    public ShouldNotHappen(string message, System.Exception inner)
        : base(message, inner) {}
}