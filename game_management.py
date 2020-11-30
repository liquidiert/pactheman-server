from fastapi import APIRouter
from pydantic import BaseModel
from typing import List, Tuple
from fastapi.responses import ORJSONResponse
from uuid import UUID

router = APIRouter()


class MoveInfo(BaseModel):
    client_id: UUID
    opponent_pos: Tuple[int]
    opponent_lives: int
    opponent_score: int
    self_pos: Tuple[int]
    self_velocity: Tuple[int]
    self_lives: int
    self_score: int
    blinky_pos: Tuple[int]
    inky_pos: Tuple[int]
    pinky_pos: Tuple[int]
    clyde_pos: Tuple[int]
    score_point_positions: List[Tuple[int]]
    tile_map: List[List[int]]


@router.post("/move", response_class=ORJSONResponse, response_model=Tuple[int], tags=["game"],
             summary="Get future velocity for opponent AI from current game situation")
def move(info: MoveInfo):
    pass

@router.delete("/disconnect", status_code=204, tags=["game"], summary="Disconnect pactheman client")
def disconnect(client_id: str):
    pass
