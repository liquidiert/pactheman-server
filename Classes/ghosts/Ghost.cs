using System;
using PacTheMan.Models;
using System.Collections.Generic;

namespace pactheman_server {

    enum GhostStates {
        Scatter,
        Chase,
        Frightened
    }

    class Ghost : Actor {

        public Ghost(){
            this.MovementSpeed = 250f;
        }
        
        public bool Waiting = true;
        protected readonly float SCATTER_SECONDS = 3.5f;
        protected float scatterTicker { get; set; }
        protected Position lastTarget { get; set; }
        protected MoveInstruction moveInstruction { get; set; }

        protected List<Position> MovesToMake;
        protected GhostStates CurrentGhostState = GhostStates.Chase;

        public override void Move() {
            if (Waiting) return;
            new ClosestAggression().SelectTarget(this);
        }

    }

}