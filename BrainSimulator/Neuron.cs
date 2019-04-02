﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BrainSimulator
{
    public class Neuron
    {

        private int currentCharge = 0;
        private int lastCharge = 0;
        internal List<Synapse> synapses = new List<Synapse>();
        internal List<Synapse> synapsesFrom = new List<Synapse>();

        int id = -1;
        string label = "";
        int range = 0; //0= [0,1]  1=[-1,1]

        public Neuron() { }
        public Neuron(int id1) { Id = id1; } //id's can get clobbered in save so here's where they can be reset

        public int Id { get => id; set => id = value; }
        public float CurrentCharge { get { return (float)currentCharge / 1000f; } set { this.currentCharge = (int)(value * 1000); } }
        public int CurrentChargeInt { get { return currentCharge; } set { this.currentCharge = value; range = 2; } }
        public float LastCharge { get { return (float)lastCharge / 1000f; } set { lastCharge = (int)(value * 1000); } }
        public int LastChargeInt { get { return lastCharge; } set { lastCharge = value;range = 2; } }

        public List<Synapse> Synapses { get { return synapses; } }
        public List<Synapse> SynapsesFrom { get { return synapsesFrom; } }

        public string Label { get { return label; } set { label = value; } }

        public int Range { get => range; set => range = value; }
        public bool Fired() { return (LastCharge > .9); }

        public void SetValue(float value)
        {
            LastCharge = CurrentCharge = value;
        }
        public void SetValueInt(int value)
        {
            LastChargeInt = CurrentChargeInt = value;
            range = 2;
        }
        public bool InUse()
        {
            return (synapses.Count != 0 || synapsesFrom.Count != 0 || Label != "");
        }

        public void AddSynapse(int targetNeuron, float weight, NeuronArray theNeuronArray, bool addUndoInfo = true)
        {
            if (targetNeuron > theNeuronArray.arraySize) return;
            Synapse s = FindSynapse(targetNeuron);
            if (s == null)
            {
                s = new Synapse(targetNeuron, weight);
                synapses.Add(s);
                if (addUndoInfo)
                    theNeuronArray.AddSynapseUndo(Id, targetNeuron, weight, true);
            }
            else
            {
                if (addUndoInfo)
                    theNeuronArray.AddSynapseUndo(Id, targetNeuron, s.Weight, false);
                s.Weight = weight;
            }
            //keep a list of synapses pointing to this one
            Neuron n = theNeuronArray.neuronArray[targetNeuron];
            s = n.FindSynapseFrom(Id);
            if (s == null)
            {
                s = new Synapse(Id, weight);
                n.synapsesFrom.Add(s);
            }
            else
            {
                s.Weight = weight;
            }
        }

        public void DeleteAllSynapes()
        {
            synapses.Clear();
            synapsesFrom.Clear();
            //should delete the synapses at the source!
        }

        public void DeleteSynapse(int targetNeuron)
        {
            synapses.Remove(FindSynapse(targetNeuron));
            Neuron n = MainWindow.theNeuronArray.neuronArray[targetNeuron];
            n.synapsesFrom.Remove(n.FindSynapseFrom(Id));
        }

        public Synapse FindSynapse(int targetNeuron)
        {
            Synapse s = synapses.Find(s1 => s1.TargetNeuron == targetNeuron);
            return s;
        }
        public Synapse FindSynapseFrom(int fromNeuron)
        {
            Synapse s = synapsesFrom.Find(s1 => s1.TargetNeuron == fromNeuron);
            return s;
        }

        //process the synapses
        bool antiFeedback = false;
        public int LastSynapse = -1;
        public void Fire1(NeuronArray theNeuronArray)
        {
            if (range == 2) return;
            if (antiFeedback)
            {
                if (lastCharge < 990) return;
                Interlocked.Add(ref theNeuronArray.fireCount, 1);
                foreach (Synapse s in synapses)
                {
                    if (s.TargetNeuron == LastSynapse)
                    { LastSynapse = -1; }
                    else
                    {
                        Neuron n = theNeuronArray.neuronArray[s.TargetNeuron];
                        Interlocked.Add(ref n.currentCharge, (int)(s.Weight * 1000));
                        if (n.currentCharge > 990 && s.Weight > 0.5f && n.Id != Id)
                            n.LastSynapse = Id;
                    }
                }
            }
            else
            {
                if (lastCharge < 990) return;
                Interlocked.Add(ref theNeuronArray.fireCount, 1);
                foreach (Synapse s in synapses)
                {
                    Neuron n = theNeuronArray.neuronArray[s.TargetNeuron];
                    Interlocked.Add(ref n.currentCharge, (int)(s.Weight * 1000));
                }

            }

        }
        //check for firing
        public void Fire2(long generation)
        {
            if (range == 2) return;
            if (range == 0 && currentCharge < 0) currentCharge = 0;
            if (range == 1 && currentCharge < -1) currentCharge = -1;
            lastCharge = currentCharge;
            if (currentCharge < 990)
            {
                return;
            }

            currentCharge = 0;
        }

        public int SynapsesTo(NeuronArray theNeuronArray)
        {
            int count = 0;
            int thisNeuron = -1;
            for (int i = 0; i < theNeuronArray.arraySize; i++)
                if (theNeuronArray.neuronArray[i] == this)
                    thisNeuron = i;
            foreach (Neuron n in theNeuronArray.neuronArray)
                foreach (Synapse s in n.synapses)
                    if (s.TargetNeuron == thisNeuron)
                        count++;
            return count;
        }
    }
}