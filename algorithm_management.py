from fastapi import APIRouter
from fastapi.responses import JSONResponse
from typing import Optional, List
from pydantic import BaseModel, types
from controllers.redis import RedisController as rc
import tempfile
import uuid
import os

router = APIRouter()


class KnownAlg(BaseModel):
    name: str
    lang: str


class AlgorithmInfo(BaseModel):
    name: str
    lang: str
    secret: str
    executable: types.Any
    code: types.Any


@router.post("/init", tags=["algorithm"],
             response_model=str,
             responses={
                400: {
                    "content": {"text/plain": {}},
                    "description": "Algorithm could not be selected"
                },
                404: {
                    "content": {"text/plain": {}},
                    "description": "Compilation failed"
                },
                409: {
                    "content": {"text/plain": {}},
                    "description": "Compilation failed"
                }
             },
             summary="Inits a new game with either a known algorithm or a completely new one",
             description="When a client inits a connection via this call the api will assign an uuid for the client.<br/> \
                New algorithms can be submitted as:<br/> \
                <ul> \
                    <li>executable binary &rarr; make example move and add binary to database</li> \
                    <li>plain code and compile language &rarr; compile code &rarr; make example move and add generated binary to database</li>\
                </ul>")
def init(known_alg: Optional[KnownAlg] = None, new_alg: Optional[AlgorithmInfo] = None):
    """
    1. see if known_alg is not None
        1.1 see if alg really known:
            if yes -> generate uuid set it with alg name in redis
            if no -> error out telling user to add alg
    2. new_alg is not None
        2.1. try compilation / injection
            if success -> add alg binary; generate uuid and set it with alg name in redis
            else -> return compile errors
    3. known_alg is None and new_alg is None -> error out
    """
    if known_alg is not None:
        if not os.path.exists(f"algorithms/{known_alg.lang}/{known_alg.name}"):
            return JSONResponse(status_code=400, content="Couldn't find requested algorithm. You may have to create it first.")
        else:
            client_id = uuid.uuid1()
            rc.hset(client_id, known_alg.__dict__)
            return client_id
    elif new_alg is not None:
        if new_alg.executable is not None:
            if "py" in new_alg.lang.lower():
                with open(f"algorithms/{new_alg.lang}/{new_alg.name}") as executable:
                    executable.write(new_alg.executable)
            else:
                with open(f"algorithms/{new_alg.lang}/{new_alg.name}", "wb") as executable:
                    executable.write(new_alg.executable)
        elif new_alg.code is not None:
            if "c" in new_alg.lang.lower():
                with tempfile.NamedTemporaryFile() as tmpFile:
                    tmpFile.write(new_alg.code)
                    os.system(f"g++ {tmpFile.name} -o algorithms/{new_alg.lang}/{new_alg.name}")
            elif "py" in new_alg.lang.lower():
                with open(f"algorithms/{new_alg.lang}/{new_alg.name}") as out_file:
                    out_file.write("#!env/bin/python3")
                    out_file.write(new_alg.code)
            else:
                return JSONResponse(status_code=404, content="Unknown programming language; couldn't compile algorithm")
        else:
            return JSONResponse(status_code=400, content="Either executable or plain code must be given")
        # TODO: test algs
        client_id = uuid.uuid1()
        rc.hset(client_id, {"name": new_alg.name, "lang": new_alg.lang})
        return client_id
    elif:
        return JSONResponse(status_code=400, content="Either known_alg or new_alg must be given")


@router.get("/alg_management", response_model=List[str], tags=["algorithm"],
            summary="Returns all known algorithm names")
def known_algs():
    pass


@router.put("/alg_management", status_code=204, tags=["algorithm"], summary="Update a known algorithms code base")
def update_alg(info: AlgorithmInfo):
    pass


@router.delete("/alg_management", status_code=204, tags=["algorithm"], summary="Delete a known algorithm")
def delete_alg(name: str, secret: str):
    pass
