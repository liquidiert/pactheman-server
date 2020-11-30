from fastapi import FastAPI
from fastapi.responses import HTMLResponse
from algorithm_management import router as alg_router
from game_management import router as game_router

app = FastAPI(
    title="pactheman API",
    description="Algorithm database and ai game handler for pactheman",
    version="0.0.1"
)

@app.get("/", response_class=HTMLResponse, include_in_schema=False)
def hello_world():
    return """
        <html>
            <body>
                Hi there and very welcome to the pactheman api!<br/>
                If you are here for the docs see <a href='/docs'>OpenAPI</a> or <a href='/redoc'>redoc</a>.
            </body>
        </html>
    """

app.include_router(alg_router)
app.include_router(game_router)