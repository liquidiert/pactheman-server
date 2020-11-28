from fastapi import APIRouter
from typing import Optional, List
from pydantic import BaseModel, types

router = APIRouter()


class AlgorithmInfo(BaseModel):
    name: str
    lang: str
    secret: str
    executable: types.Any
    code: types.Any


@router.post("/init", tags=["algorithm"],
             response_model=str,
             summary="Inits a new game with either a known algorithm or a completely new one",
             description="When a client inits a connection via this call the api will assign an uuid for the client.<br/> \
                New algorithms can be submitted as:<br/> \
                <ul> \
                    <li>executable binary &rarr; make example move and add binary to database</li> \
                    <li>plain code and compile language &rarr; compile code &rarr; make example move and add generated binary to database</li>\
                </ul>")
def init(alg_name: Optional[str] = None, new_alg: Optional[AlgorithmInfo] = None):
    print(new_alg)


@router.get("/alg_management", response_model=List[str], tags=["algorithm"],
            summary="Returns all known algorithm names")
def known_algs():
    pass


@router.put("/alg_management", tags=["algorithm"], summary="Update a known algorithms code base")
def update_alg(info: AlgorithmInfo):
    pass


@router.delete("/alg_management", tags=["algorithm"], summary="Delete a known algorithm")
def delete_alg(name: str, secret: str):
    pass
