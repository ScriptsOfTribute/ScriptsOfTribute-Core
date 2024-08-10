using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bots;

public class TreeReporterSOISMCTS
{
    public static void ReportTreeStructure(InfosetNode root, double UCB_K, StreamWriter logWriter)
    {
        Queue<(InfosetNode node, int layer, Move? move)> queue = new Queue<(InfosetNode node, int layer, Move? move)>();
        queue.Enqueue((root, 0, null));
        
        Dictionary<int, List<string>> layers = new Dictionary<int, List<string>>();

        while (queue.Count > 0)
        {
            var (currentNode, currentLayer, move) = queue.Dequeue();
            
            string nodeInfo = GetNodeInfo(currentNode, move, UCB_K);
            
            if (!layers.ContainsKey(currentLayer))
            {
                layers[currentLayer] = new List<string>();
            }

            foreach (var child in currentNode.Children)
            {
                queue.Enqueue((child, currentLayer + 1, child.GetCurrentMoveFromParent()));
            }
        }
        
        //PrintTreeReport(layers);
        WriteTreeReportToLog(layers, logWriter);
    }

    private static string GetNodeInfo(InfosetNode node, Move? move, double UCB_K)
    {
        // if (move is null && node.Parent is not null)
        // {
        //     int i = 0;
        // }
        string refMoveHistory = "";
        if (node._refMoveHistory.Count > 0)
        {
            for (int i = 0; i < node._refMoveHistory.Count; i++)
            {
                refMoveHistory += node._refMoveHistory[i].ToString() + ",";
            }
        }
        else
        {
            refMoveHistory = "Zero moves";
        }

        string currentAvailableMoves = "";
        if (node.GetCurrentDeterminisation() is not null)
        {
            List<Move> availableMoves = node.GetCurrentDeterminisation().GetMoves();
            currentAvailableMoves = availableMoves.Count.ToString();
            for (int i = 0; i < availableMoves.Count; i++)
            {
                //currentAvailableMoves += availableMoves[i].ToString() + ",";
            }
        }
        else
        {
            currentAvailableMoves = "Current determinisation is null";
        }

        string moveInfo = move != null ? MoveToString(move) : "Root";
        // string nodeInfo = $"Hashcode: {node.GetHashCode()}, Reference move history: {refMoveHistory}, Move From Parent: {moveInfo}, UCB: {node.UCB(UCB_K):F3}, MaxReward: {node.MaxReward:F3}, " +
        //                   $"VisitCount: {node.VisitCount}, AvailabilityCount: {node.AvailabilityCount}, Current Available Moves: {currentAvailableMoves}";
        
        string parentHashCode = "Root";
        if (node.Parent is not null)
            parentHashCode = node.Parent.GetHashCode().ToString();
        
        string nodeInfo = $"Hashcode: {node.GetHashCode()}, Parent Hashcode: {parentHashCode}, Reference move history: {refMoveHistory}, Move From Parent: {moveInfo}, " +
                          $"MaxReward: {node.MaxReward}, AvgReward: {node.AvgReward}, UCBExploration: {node.UCBExploration}, VisitCount: {node.VisitCount}," +
                          $"AvailabilityCount: {node.AvailabilityCount}, No of Current Available Moves: {currentAvailableMoves}";
        
        return nodeInfo;
    }

    private static string MoveToString(Move move)
    {
        // Implement a way to convert a Move object to a string representation
        return move.ToString(); // Adjust this as necessary for your Move object
    }

    private static void PrintTreeReport(Dictionary<int, List<string>> layers)
    {
        foreach (var layer in layers)
        {
            Console.WriteLine($"Layer {layer.Key}:");
            foreach (var nodeInfo in layer.Value)
            {
                Console.WriteLine($"  {nodeInfo}");
            }
        }
    }
    
    private static void WriteTreeReportToLog(Dictionary<int, List<string>> layers, StreamWriter logWriter)
    {
        foreach (var layer in layers)
        {
            logWriter.WriteLine($"Layer {layer.Key}:");
            foreach (var nodeInfo in layer.Value)
            {
                logWriter.WriteLine($"  {nodeInfo}");
            }
        }
    }
}