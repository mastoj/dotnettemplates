open Saturn
open Giraffe
open FSharp.Control.Tasks.ContextInsensitive

module TodosApi =
    type CreateTodo = {
        Title: string
    }

(*
POST http://localhost:5000/api/todos/
Content-Type: application/json

{
    title: "yolo"
}
*)

    let createTodo ctx = task {
        let! input = Controller.getModel<CreateTodo> ctx
        return! Controller.json ctx input 
    }

(*
GET http://localhost:5000/api/todos/123
*)
    let showTodo ctx (id: string) = task {
        return! Controller.json ctx {|Title = "Hello"; Id = id |}
    }

(*
GET http://localhost:5000/api/todos/
*)
    let showAllTodos ctx = task {
        return! Controller.json ctx [{|Title = "Hello"; Id = "yolo world" |}]
    }

(*
POST http://localhost:5000/api/todos/21123
Content-Type: application/json

{
    title: "yolo"
}
*)
    let updateTodo ctx (id: string) = task {
        let! input = Controller.getModel<CreateTodo> ctx
        return! Controller.json ctx input 
    }

    let controller = controller {
        index showAllTodos
        create createTodo
        show showTodo
        update updateTodo
    }


let apiRouter =
    router {
        forward "/todos" TodosApi.controller
    }

let app = application {
    use_router (router {
        forward "/api" apiRouter
        get "/" (text "Hello from root")
    })
}

run app