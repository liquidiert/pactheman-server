using System.Threading.Tasks;
using PacTheMan.Models;
using System.Collections.Generic;
using System;

namespace pactheman_server {

    enum GhostStates {
        Scatter,
        Chase,
        Frightened
    }

    class Ghost : Actor {

        public Ghost(MoveInstruction instruction){
            this.MovementSpeed = 250f;
            this.moveInstruction = instruction;
        }

        
        public bool Waiting = true;

        protected double delta = 0.016666667;
        protected readonly float SCATTER_SECONDS = 3.5f;
        protected float scatterTicker { get; set; }
        protected Position lastTarget { get; set; }
        protected MoveInstruction moveInstruction { get; set; }

        protected List<Position> MovesToMake;
        protected GhostStates CurrentGhostState = GhostStates.Chase;

        public virtual async Task<dynamic> Move(Player TargetOne, Player TargetTwo) {
            await Task.Yield();
            if (this.Position.IsEqualUpToRange(TargetOne.Position)) {
                return new Tuple<Boolean, Player>(true, TargetOne);
            } else if (this.Position.IsEqualUpToRange(TargetTwo.Position)) {
                return new Tuple<Boolean, Player>(true, TargetTwo);
            }
            Position = StartPosition;
            return new Tuple<Boolean, Player>(false, null);
        }

    }

}