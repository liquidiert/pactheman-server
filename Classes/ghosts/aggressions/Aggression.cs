namespace pactheman_server {
    public interface Aggression {
        Actor SelectTarget(Actor aggressor, Actor posT1, Actor posT2);
        
    }
}