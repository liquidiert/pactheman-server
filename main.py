from fastapi import FastAPI
from algorithm_management import router as alg_router
from game_management import router as game_router

app = FastAPI(
    title="pactheman API",
    description="Algorithm database and ai game handler for pactheman",
    version="0.0.1"
)
app.include_router(alg_router)
app.include_router(game_router)