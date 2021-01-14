using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using PacTheMan.Models;

namespace pactheman_server {
    public class PacTheManClient : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private OrthographicCamera _camera;

        // menus
        private MainMenu _mainMenu;

        // environment
        private TiledMap map;
        private TiledMapRenderer mapRenderer;
        private GameEnv GameEnv;

        // characters
        private HumanPlayer player;
        private Opponent opponent;
        private Ghost pinky;
        private Ghost blinky;
        private Ghost inky;
        private Ghost clyde;

        public PacTheManClient() {
            _graphics = new GraphicsDeviceManager(this) { IsFullScreen = false };
            GameState.Instance.CurrentGameState = GameStates.MainMenu;
            UIState.Instance.CurrentUIState = UIStates.MainMenu;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            ContentTypeReaderManager.AddTypeCreator("Default", () => new JsonContentTypeReader<TexturePackerFile>());
        }

        private void WindowOnClientSizeChanged(object sender, EventArgs eventArgs) {
            UIState.Instance.GuiSystem.ClientSizeChanged();
        }

        protected override async void OnExiting(Object sender, EventArgs args) {
            base.OnExiting(sender, args);
            await GameEnv.Instance.Exit();
        }

        protected override void Initialize() {
            // _camera
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 2216, 1408);
            _camera = new OrthographicCamera(viewportAdapter);
            _camera.LookAt(new Vector2(608, 704));

            // menus
            _mainMenu = new MainMenu(Exit);

            // gui rendering
            var font = Content.Load<BitmapFont>("fonts/go");
            BitmapFont.UseKernings = false;
            Skin.CreateDefault(font);
            var guiRenderer = new GuiSpriteBatchRenderer(GraphicsDevice, viewportAdapter.GetScaleMatrix);
            UIState.Instance.GuiSystem = new GuiSystem(viewportAdapter, guiRenderer);
            UIState.Instance.CurrentScreen = UIState.Instance.MainMenu = _mainMenu;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;

            base.Initialize();
        }

        private SessionListener CreateSessionListener() {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                IPAddress address;
                if (args[0] == "localhost") args[0] = "127.0.0.1";
                if (!IPAddress.TryParse(args[0], out address)) {
                    Console.WriteLine("Error: invalid ip address");
                }
                if (args.Length < 2) {
                    int port;
                    if (!int.TryParse(args[1], out port)) {
                        Console.WriteLine("Error: port must be a number");
                    }
                    return new SessionListener(address, port);
                }
                return new SessionListener(address);
            }
            return new SessionListener(IPAddress.Parse("127.0.0.1"));
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // tile map
            map = Content.Load<TiledMap>("pactheman_map");
            mapRenderer = new TiledMapRenderer(GraphicsDevice, map);

            GameEnv = GameEnv.Instance.Init(Content, map);

            // actors
            player = new HumanPlayer(Content, "PlayerOne");

            opponent = new Opponent(Content, "PlayerTwo");

            pinky = new Pinky(Content, "pinky");
            blinky = new Blinky(Content, "blinky");
            inky = new Inky(Content, "inky");
            clyde = new Clyde(Content, "clyde");

            GameEnv.Actors.TryAdd("player", player);
            GameEnv.Actors.TryAdd("opponent", opponent);
            GameEnv.Actors.TryAdd(pinky.Name, pinky);
            GameEnv.Actors.TryAdd(blinky.Name, blinky);
            GameEnv.Actors.TryAdd(clyde.Name, clyde);
            GameEnv.Actors.TryAdd(inky.Name, inky);

            // add collisions
            GameEnv.AddCollisions();

            Task listener = CreateSessionListener().Listen();

            base.LoadContent();
        }

        protected override async void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                if (UIState.Instance.CurrentUIState == UIStates.Game) {
                    // TODO: check if online if so send pause
                    UIState.Instance.CurrentUIState = UIStates.InGame;
                    UIState.Instance.GuiSystem.ActiveScreen.Show();
                    GameState.Instance.CurrentGameState = GameStates.GamePaused;
                }
            }

            switch (GameState.Instance.CurrentGameState) {
                case GameStates.Game:
                    // update map
                    mapRenderer.Update(gameTime);

                    // update actors
                    foreach (var actor in GameEnv.Actors.Values) {
                        actor.Move(gameTime);
                        actor.Sprite.Update(gameTime);
                    }

                    // update collision pairs
                    foreach (var pair in GameEnv.Instance.CollisionPairs) {
                        pair.Update();
                    }

                    await GameEnv.Instance.Session.SendGhostPositions(
                        new GhostMoveMsg {
                            GhostPositions = GameEnv.Instance.Ghosts
                                .ToDictionary(gP => gP.Name, gP => (BasePosition)gP.Position.ToPosition())
                        }
                    );
                    break;
                case GameStates.GameReset:
                    GameState.Instance.RESET_COUNTER -= gameTime.GetElapsedSeconds();
                    if (GameState.Instance.RESET_COUNTER <= 0) {
                        GameState.Instance.CurrentGameState = GameStates.Game;
                        GameState.Instance.RESET_COUNTER = 4f;
                    }
                    break;

            }
            if (UIState.Instance.CurrentUIState != UIStates.Game) {
                UIState.Instance.CurrentScreen.Update(gameTime);
            }

            UIState.Instance.GuiSystem.Update(gameTime);
            GameEnv.Walls.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // map draw
            _spriteBatch.Begin(
                transformMatrix: _camera.GetViewMatrix(),
                samplerState: new SamplerState { Filter = TextureFilter.Point }
            );

            switch (GameState.Instance.CurrentGameState) {
                case GameStates.GamePaused:
                case GameStates.Game:
                    DrawGameEnv();
                    break;
                case GameStates.GameReset:
                    DrawGameEnv();
                    _spriteBatch.DrawString(
                        Content.Load<SpriteFont>("ScoreFont"),
                        $"{(int)GameState.Instance.RESET_COUNTER}",
                        new Vector2(570, 650),
                        Color.Yellow,
                        0f,
                        Vector2.Zero,
                        3f,
                        SpriteEffects.None,
                        0f
                    );
                    break;
            }
            _spriteBatch.End();

            if (UIState.Instance.CurrentUIState != UIStates.Game) {
                UIState.Instance.GuiSystem.Draw(gameTime);
            }


            base.Draw(gameTime);
        }

        private void DrawGameEnv() {
            // draw map
            mapRenderer.Draw(_camera.GetViewMatrix());

            // draw score points
            foreach (var point in GameEnv.ScorePointPositions) {
                point.Draw(_spriteBatch);
            }

            // player stats
            DrawPlayerStats(player);
            DrawPlayerStats(opponent);

            // draw actors
            foreach (var actor in GameEnv.Actors.Values) {
                actor.Draw(_spriteBatch);
            }
            /*
            // debug bounding box
            Texture2D _texture;

            _texture = new Texture2D(GraphicsDevice, 1, 1);
            _texture.SetData(new Color[] { Color.DarkSlateGray });
            _spriteBatch.Draw(_texture, 
                new Rectangle((int) blinky.BoundingBox.X, (int) blinky.BoundingBox.Y, (int) blinky.BoundingBox.Width, (int) blinky.BoundingBox.Height),
                Color.White); */
        }

        private void DrawPlayerStats(Player player) {
            // player one stats
            // draw name
            _spriteBatch.DrawString(
                Content.Load<SpriteFont>("ScoreFont"),
                player.Name,
                player.StatsPosition,
                Color.White
            );
            // draw lives
            _spriteBatch.DrawString(
                Content.Load<SpriteFont>("ScoreFont"),
                $"Lives: {player.Lives}",
                new Vector2(player.StatsPosition.X, 100),
                Color.White
            );
            // draw score
            _spriteBatch.DrawString(
                Content.Load<SpriteFont>("ScoreFont"),
                $"Score: {player.Score}",
                new Vector2(player.StatsPosition.X, 150),
                Color.White
            );
        }
    }
}
