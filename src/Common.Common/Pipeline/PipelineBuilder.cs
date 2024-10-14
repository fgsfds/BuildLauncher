//namespace Common.Common.Pipeline;

//public sealed class PipelineBuilder<T>
//{
//    private readonly List<IPipelineStep<T>> _steps;

//    public PipelineBuilder()
//    {
//        _steps = [];
//    }

//    internal PipelineBuilder<T> AddStep(IPipelineStep<T> pipelineStep1)
//    {
//        _steps.Add(pipelineStep1);
//        return this;
//    }

//    internal PipelineExecutor<T> Build()
//    {
//        return new(_steps);
//    }
//}



//public sealed class PipelineExecutor<T>
//{
//    private readonly List<IPipelineStep<T>> _steps;

//    public PipelineExecutor(List<IPipelineStep<T>> steps)
//    {
//        _steps = steps;
//    }

//    public T Execute()
//    {

//    }
//}




//public interface IPipelineStep<T>
//{
//    T Process(T obj);
//}

//public sealed class StringToUpper : IPipelineStep<string>
//{
//    public string Process(string obj)
//    {
//        throw new NotImplementedException();
//    }
//}




//public class MM
//{
//    public void Method()
//    {
//        var builder = new PipelineBuilder<string>()
//            .AddStep(new StringToUpper())
//            .AddStep(new StringToUpper())
//            .Build();

//        var result = builder.Execute();
//    }
//}
