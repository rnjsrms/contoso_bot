﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Bot_Application.Services;

namespace Bot_Application
{
    public class IVRBot : IDisposable, ICallingBot
    {
        private const string Support = "1";

        // Response messages depending on user selection
        private const string WelcomeMessage = "Hello, welcome to contoso bot speech service";
        private const string MainMenuPromptMessage = "please press 1 to begin your service.";
        private const string NoConsultantsMessage = "We currently have following services for you. Deposit, Withdraw, balance check, finding exchange rate of currencies, finding stock price of companies, display current news, deleting account. Please select or type one of the services. For your convinience, your voice will be translated into text. Please press the hash key when finished.";
        private const string EndingMessage = "Thank you for leaving the message, goodbye";
        private const string OptionMenuNotSupportedMessage = "The option you entered is not supported. Please try again.";

        private readonly Dictionary<string, CallState> callStateMap = new Dictionary<string, CallState>();

        private readonly MicrosoftCognitiveSpeechService speechService = new MicrosoftCognitiveSpeechService();

        public IVRBot(ICallingBotService callingBotService)
        {
            if (callingBotService == null)
            {
                throw new ArgumentNullException(nameof(callingBotService));
            }

            this.CallingBotService = callingBotService;

            this.CallingBotService.OnIncomingCallReceived += this.OnIncomingCallReceived;
            this.CallingBotService.OnPlayPromptCompleted += this.OnPlayPromptCompleted;
            this.CallingBotService.OnRecordCompleted += this.OnRecordCompleted;
            this.CallingBotService.OnRecognizeCompleted += this.OnRecognizeCompleted;
            this.CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        public ICallingBotService CallingBotService { get; }

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.OnIncomingCallReceived;
                this.CallingBotService.OnPlayPromptCompleted -= this.OnPlayPromptCompleted;
                this.CallingBotService.OnRecordCompleted -= this.OnRecordCompleted;
                this.CallingBotService.OnRecognizeCompleted -= this.OnRecognizeCompleted;
                this.CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        private static Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static void SetupInitialMenu(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase> { GetInitialMenu() };
        }

        private static void SetupInitialMenuWithErrorMessage(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase>
            {
                GetPromptForText(OptionMenuNotSupportedMessage),
                GetInitialMenu()
            };
        }

        private static ActionBase GetInitialMenu()
        {
            return CreateIvrOptions(MainMenuPromptMessage, 1, false);
        }

        private static void ProcessMainMenuSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                if (outcome.RecognizeOutcome.FailureReason == RecognitionCompletionReason.IncorrectDtmf.ToString())
                {
                    SetupInitialMenuWithErrorMessage(outcome.ResultingWorkflow);
                }
                else
                {
                    SetupInitialMenu(outcome.ResultingWorkflow);
                }

                return;
            }

            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case Support:
                    callStateForClient.ChosenMenuOption = Support;
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private static Recognize CreateIvrOptions(string textToBeRead, int numberOfOptions, bool includeBack)
        {
            if (numberOfOptions > 9)
            {
                throw new Exception("too many options specified");
            }

            var choices = new List<RecognitionOption>();

            for (int i = 1; i <= numberOfOptions; i++)
            {
                choices.Add(new RecognitionOption { Name = Convert.ToString(i), DtmfVariation = (char)('0' + i) });
            }

            if (includeBack)
            {
                choices.Add(new RecognitionOption { Name = "#", DtmfVariation = '#' });
            }

            var recognize = new Recognize
            {
                OperationId = Guid.NewGuid().ToString(),
                PlayPrompt = GetPromptForText(textToBeRead),
                BargeInAllowed = true,
                Choices = choices
            };

            return recognize;
        }

        private static void SetupRecording(Workflow workflow)
        {
            var id = Guid.NewGuid().ToString();

            var prompt = GetPromptForText(NoConsultantsMessage);
            var record = new Record
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 60,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 4,
                PlayBeep = true,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
            workflow.Actions = new List<ActionBase> { record };
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            this.callStateMap[incomingCallEvent.IncomingCall.Id] = new CallState(incomingCallEvent.IncomingCall.Participants);

            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = Guid.NewGuid().ToString() },
                    GetPromptForText(WelcomeMessage)
                };

            return Task.FromResult(true);
        }

        private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var callState = this.callStateMap[playPromptOutcomeEvent.ConversationResult.Id];
            SetupInitialMenu(playPromptOutcomeEvent.ResultingWorkflow);

            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText(EndingMessage),
                    new Hangup { OperationId = Guid.NewGuid().ToString() }
                };

            // Convert the audio to text
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                var text = await this.GetTextFromAudioAsync(record);

                var callState = this.callStateMap[recordOutcomeEvent.ConversationResult.Id];

                await this.SendSTTResultToUser("You said: " + text, callState.Participants);

            }

            recordOutcomeEvent.ResultingWorkflow.Links = null;
            this.callStateMap.Remove(recordOutcomeEvent.ConversationResult.Id);
        }

        private async Task SendSTTResultToUser(string text, IEnumerable<Participant> participants)
        {
            var to = participants.Single(x => x.Originator);
            var from = participants.First(x => !x.Originator);

            await AgentListener.Resume(to.Identity, to.DisplayName, from.Identity, from.DisplayName, to.Identity, text);
        }

        /// <summary>
        /// Gets text from an audio stream.
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns>Transcribed text. </returns>
        private async Task<string> GetTextFromAudioAsync(Stream audiostream)
        {
            var text = await this.speechService.GetTextFromAudioAsync(audiostream);
            Debug.WriteLine(text);
            return text;
        }

        private Task OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
        {
            var callState = this.callStateMap[recognizeOutcomeEvent.ConversationResult.Id];

            ProcessMainMenuSelection(recognizeOutcomeEvent, callState);

            return Task.FromResult(true);
        }

        private class CallState
        {
            public CallState(IEnumerable<Participant> participants)
            {
                this.Participants = participants;
            }

            public string ChosenMenuOption { get; set; }

            public IEnumerable<Participant> Participants { get; }
        }
    }
}