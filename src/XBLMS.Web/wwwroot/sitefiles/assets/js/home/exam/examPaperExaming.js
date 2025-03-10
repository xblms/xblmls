var $url = "/exam/examPaperExaming";
var $urlItem = $url+ "/item";
var $urlSubmitAnswer = $url +"/submitAnswer";
var $urlSubmitPaper = $url + "/submitPaper";
var $urlSubmitTiming = $url + "/submitTiming";

var data = utils.init({
  id: utils.getQueryInt('id'),
  planId: utils.getQueryInt('planId'),
  courseId: utils.getQueryInt('courseId'),
  startId:0,
  list: null,
  paper: null,
  tm: null,
  watermark:null,
  answerTotal: 0,
  tmAnswerStatus:false,
  tmList: [],
  surplusSecond: 0,
  curTimingSecond:1
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true, "正在加载试卷...");

    $api.get($url, { params: { id: $this.id, planId: this.planId, courseId: this.courseId } }).then(function (response) {
      var res = response.data;

      $this.watermark = res.watermark;
      $this.paper = res.item;
      $this.list = res.txList;

      $this.startId = $this.paper.startId;

      if ($this.paper.isTiming) {
        if ($this.paper.useTimeSecond > -1) {
          $this.surplusSecond = Date.now() + $this.paper.timingMinute * 60 * 1000 - $this.paper.useTimeSecond * 1000;
          if ($this.surplusSecond > 0) {
            $this.timingChange();
          }
          else {
            $this.btnSubmitPaperClick();
          }
        }
        else {
          $this.btnSubmitPaperClick();
        }
      }

      if ($this.list && $this.list.length > 0) {
        $this.list.forEach(item => {
          var cTmList = item.tmList;
          if (cTmList && cTmList.length > 0) {
            cTmList.forEach(ctm => {
              $this.tmList.push(ctm);
              if (ctm.answerStatus) {
                $this.answerTotal++;
              }
            })
          }

        });

        $this.btnGetTm($this.tmList[0].id);
      }

    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
      utils.loading($this, false);
    });
  },
  btnGetTm(id) {
    this.tm = null;
    var $this = this;
    $this.$nextTick(() => {
      setTimeout(function () {
        var getCurTm = $this.tmList.find(item => item.id === id);
        $this.tm = getCurTm;
      }, 200);
    })
  },
  getTmAnswerStatus: function (id) {
    var getCurTm = this.tmList.find(item => item.id === id);
    return getCurTm.answerStatus;
  },
  answerChange: function () {

    let setTm = this.tm;

    this.tmList = this.tmList.filter(f => f.id !== setTm.id);
    setTm.answerStatus = false;

    if (setTm.baseTx === "Duoxuanti") {
      setTm.answerInfo.answer = setTm.answerInfo.optionsValues.join('');
    }
    var completionStatus = true;
    if (setTm.baseTx === "Tiankongti") {
      for (var i = 0; i < setTm.answerInfo.optionsValues.length; i++) {
        if (setTm.answerInfo.optionsValues[i] === '' || setTm.answerInfo.optionsValues[i] === null) {
          completionStatus = false;
        }
      }
      setTm.answerInfo.answer = setTm.answerInfo.optionsValues.join(',');
    }

    if (setTm.answerInfo.answer !== '' && setTm.answerInfo.answer.length > 0) {
      if (completionStatus) {
        setTm.answerStatus = true;
      }
      else {
        setTm.answerStatus = false;
      }
    }

    this.tmList.push(setTm);


    var answerTotals = this.tmList.filter(f => f.answerStatus);
    this.answerTotal = answerTotals.length;

    this.apiSubmitAnswer(setTm.answerInfo);
  },
  btnDownClick: function () {
    var curIndex = this.tm.tmIndex;
    let downTm = this.tmList.find(item => item.tmIndex === (curIndex + 1))
    this.btnGetTm(downTm.id);
  },
  btnUpClick: function () {
    var curIndex = this.tm.tmIndex;
    let upTm = this.tmList.find(item => item.tmIndex === (curIndex - 1))
    this.btnGetTm(upTm.id);
  },
  apiSubmitAnswer: function (setTm) {
    $api.post($urlSubmitAnswer, { answer: setTm }).then(function (response) { });
  },
  apiSubmitPaper: function () {
    $api.post($urlSubmitPaper, { id: this.startId }).then(function (response) { });
  },
  apiSubmitTiming: function () {
    $api.post($urlSubmitTiming, { id: this.startId }).then(function (response) { });
  },
  btnSubmitPaperClick: function () {
    this.apiSubmitPaper();
    location.href = utils.getExamUrl("examPaperSubmitResult", { id: this.startId });
  },
  btnPaperSubmit: function () {
    var $this = this;
    top.utils.alertWarning({
      title: '交卷提醒',
      text: '立即交卷，确定吗？',
      callback: function () {
        $this.btnSubmitPaperClick();
      }
    });
  },
  timingFinish: function () {
    this.btnSubmitPaperClick();
  },
  timingChange: function () {
    if (this.curTimingSecond % 5 === 0) {
      this.apiSubmitTiming();
    }
    var $this = this;
    setTimeout(function () {
      $this.curTimingSecond++;
      $this.timingChange();
    },1000)
  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    utils.loading(this, false);
    this.apiGet();
  },
});
