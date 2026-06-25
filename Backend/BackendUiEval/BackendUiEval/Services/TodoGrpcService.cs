using BackendUiEval.Grpc;
using Grpc.Core;

namespace BackendUiEval.Services;

public class TodoGrpcService : Grpc.TodoService.TodoServiceBase
{
    private readonly TodoStore _store;

    public TodoGrpcService(TodoStore store) => _store = store;

    public override Task<Todo> CreateTodo(Todo request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "description is required"));
        if (request.Person is Grpc.Person.Unspecified)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "person is required"));
        if (request.Type is Grpc.TodoType.Unspecified)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "type is required"));
        if (request.State is Grpc.TodoState.Unspecified)
            request.State = Grpc.TodoState.Ready;

        return Task.FromResult(_store.Add(request));
    }

    public override Task<ListTodosResponse> ListAll(ListAllRequest request, ServerCallContext context)
    {
        var (all, total) = _store.GetAll();
        var resp = new ListTodosResponse { Total = total };
        resp.Todos.AddRange(all);
        return Task.FromResult(resp);
    }

    public override Task<ListTodosResponse> ListPaged(ListPagedRequest request, ServerCallContext context)
    {
        var (page, total) = _store.GetPaged(request.Offset, request.Limit);
        var resp = new ListTodosResponse { Total = total };
        resp.Todos.AddRange(page);
        return Task.FromResult(resp);
    }
}
